using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace FactoryFramework
{
    public static class BeltMeshGenerator
    {
        public static Mesh Generate(IPath path, BeltMeshSO model, float segments, float scaleFactor, float uvScaleFactor = 1f, bool generateBeltUVS = false)
        {
            if (segments < 3)
                Debug.LogWarning("Generating a mesh with less than 3 segments can lead to unexpected results or errors!");

            // Check if path type supports multithreaded mesh generation
            if (path as IPathMeshGenerator != null)
            {
                return GenerateJob(path, model, (int)segments, scaleFactor, uvScaleFactor, generateBeltUVS);
            }
            else //revert to single threaded generation
            {
                return GenerateSingleThread(path, model, (int)segments, scaleFactor, uvScaleFactor, generateBeltUVS);
            }
        }
        public static Mesh GenerateSingleThread(IPath path, BeltMeshSO model, int segments, float scaleFactor, float uvScaleFactor = 1f, bool generateBeltUVS = false)
        {
            List<Vector3> verts = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();
            float len = path.GetTotalLength();
            float perSegment = 1f / segments;
            SerializableMesh current = model.startCap;

            int triOffset = 0;

            float unitperuv = uvScaleFactor;
            float uvperpath = len / unitperuv;

            for (int i = 0; i < segments; i++)
            {
                if (i >= segments - 1)
                {
                    current = model.endCap;
                }
                else if (i > 0)
                {
                    current = model.midSegment;
                }
                float segStart = i * perSegment;
                float segEnd = (i + 1) * perSegment;
                float meshStart = current.GetMesh().bounds.min.z;
                float meshEnd = current.GetMesh().bounds.max.z;

                int index = 0;
                foreach (Vector3 v in current.GetMesh().vertices)
                {
                    float pathSpaceZ = Remap(v.z, meshStart, meshEnd, segStart, segEnd);
                    Vector3 offsetPos = path.GetWorldPointFromPathSpace(pathSpaceZ);
                    Quaternion rot = path.GetRotationAtPoint(pathSpaceZ);
                    Vector3 rotatedMeshPos = rot * new Vector3(v.x * scaleFactor, v.y * scaleFactor, 0);
                    verts.Add(offsetPos + rotatedMeshPos);
                    normals.Add(current.GetMesh().normals[index]);
                    if (generateBeltUVS)
                        uvs.Add(new Vector2(pathSpaceZ * uvperpath, current.GetMesh().uv[index].y));
                    else
                        uvs.Add(current.GetMesh().uv[index]);
                    index++;
                }
                foreach (int t in current.GetMesh().triangles)
                {
                    tris.Add(t + triOffset);
                }
                triOffset = verts.Count;
            }

            Mesh m = new Mesh();
            m.vertices = verts.ToArray();
            m.triangles = tris.ToArray();
            m.normals = normals.ToArray();
            m.uv = uvs.ToArray();
            return m;
        }
        // Run mesh generation using Unity Jobs + Burst
        public static Mesh GenerateJob(IPath path, BeltMeshSO model, int segments, float scaleFactor, float uvScaleFactor = 1f, bool generateBeltUVS = false)
        {
            MeshGenParams settings = new MeshGenParams()
            {
                len = path.GetTotalLength(),
                perSegment = 1f / segments,
                uvperpath = path.GetTotalLength() / uvScaleFactor,
                segments = segments,
                scaleFactor = scaleFactor,
                beltUvs = generateBeltUVS,
            };

            NativeMeshGroup inputMesh = new NativeMeshGroup(model.startCap.GetMesh(), model.midSegment.GetMesh(), model.endCap.GetMesh());
            NativeMesh outputMesh = new NativeMesh(inputMesh.GetTotalVerts(segments), inputMesh.GetTotalTris(segments));

            JobHandle jobHandle = ((IPathMeshGenerator)path).RunMeshGenJob(ref inputMesh, ref outputMesh, ref settings);
            jobHandle.Complete();

            Mesh m = new Mesh();
            m.vertices = outputMesh.verts.ToArray();
            m.normals = outputMesh.normals.ToArray();
            m.uv = outputMesh.uvs.ToArray();
            m.triangles = outputMesh.tris.ToArray();

            inputMesh.Dispose();
            outputMesh.Dispose();
            return m;
        }

        public static class MeshGenApi
        {
            // Used by IPathMeshGenerator implementors to create a MeshGenConfig job according to their internal struct 
            public static MeshGenConfig<T> CreateMeshGenJob<T>(T pathStruct, ref NativeMeshGroup inputMesh, ref NativeMesh outputMesh, ref MeshGenParams settings) where T : struct, IPath
            {
                return new MeshGenConfig<T> { pStruct = pathStruct, inputMesh = inputMesh, outputMesh = outputMesh, settings = settings };
            }
        }

        // The actual mesh generation job
        public partial struct MeshGenConfig<T> where T : struct, IPath
        {
            public T pStruct;
            public NativeMeshGroup inputMesh;
            public NativeMesh outputMesh;
            public MeshGenParams settings;

            public JobHandle Run()
            {
                return new MeshGenJob { pStruct = pStruct, inputMesh = inputMesh, outputMesh = outputMesh, settings = settings }.Schedule(settings.segments, 8); // 8 is batch size
            }

            [BurstCompile]
            public struct MeshGenJob : IJobParallelFor
            {
                [ReadOnly, NativeDisableContainerSafetyRestriction]
                public T pStruct;

                [ReadOnly, NativeDisableContainerSafetyRestriction]
                public NativeMeshGroup inputMesh;

                [WriteOnly, NativeDisableParallelForRestriction]
                public NativeMesh outputMesh;

                [ReadOnly, NativeDisableContainerSafetyRestriction]
                public MeshGenParams settings;

                public void Execute(int segIndex)
                {
                    // Start and end points in pathspace (0-1) for this segment
                    float segStart = segIndex * settings.perSegment;
                    float segEnd = (segIndex + 1) * settings.perSegment;

                    // Holder for our current mesh (start, mid, end)
                    ref NativeMesh curMesh = ref inputMesh.mid;
                    int meshIndex = 1;
                    if (segIndex == 0)
                    {
                        meshIndex = 0;
                        curMesh = ref inputMesh.start;
                    }
                    else if (segIndex == settings.segments - 1)
                    {
                        meshIndex = 2;
                        curMesh = ref inputMesh.end;
                    }

                    int index = 0;
                    int triCount = 0;
                    int offset = 0;
                    if (meshIndex != 0)
                    {
                        // Calculate our current triangle offset and vertex offset in the output array
                        triCount = (segIndex - 1) * inputMesh.mid.tris.Length + inputMesh.start.tris.Length;
                        offset = (segIndex - 1) * inputMesh.mid.verts.Length + inputMesh.start.verts.Length;
                    }

                    // Convert each vertex in the input mesh to an appropriate vertex along the path
                    foreach (Vector3 v in curMesh.verts)
                    {
                        float pathSpaceZ = Remap(v.z, curMesh.zMin, curMesh.zMax, segStart, segEnd);
                        Vector3 offsetPos = pStruct.GetWorldPointFromPathSpace(pathSpaceZ);
                        Quaternion rot = pStruct.GetRotationAtPoint(pathSpaceZ);
                        Vector3 rotatedMeshPos = rot * new Vector3(v.x * settings.scaleFactor, v.y * settings.scaleFactor, 0);
                        outputMesh.verts[offset + index] = (offsetPos + rotatedMeshPos);
                        outputMesh.normals[offset + index] = curMesh.normals[index];
                        if (settings.beltUvs)
                            outputMesh.uvs[offset + index] = new Vector2(pathSpaceZ * settings.uvperpath, curMesh.uvs[index].y);
                        else
                            outputMesh.uvs[offset + index] = curMesh.uvs[index];

                        index++;
                    }

                    // Assign triangles
                    index = 0;
                    foreach (int t in curMesh.tris)
                    {
                        outputMesh.tris[triCount + index] = t + offset;
                        index++;
                    }
                }
            }
        }

        // Struct for holding mesh data for use with Jobs
        public struct NativeMesh : IDisposable
        {
            [NativeDisableParallelForRestriction]
            public NativeArray<Vector3> verts;
            [NativeDisableParallelForRestriction]
            public NativeArray<Vector2> uvs;
            [NativeDisableParallelForRestriction]
            public NativeArray<Vector3> normals;
            [NativeDisableParallelForRestriction]
            public NativeArray<int> tris;

            public float zMin;
            public float zMax;

            public NativeMesh(Mesh m) : this(m.vertices, m.uv, m.normals, m.triangles)
            {
                zMin = m.bounds.min.z;
                zMax = m.bounds.max.z;
            }
            public NativeMesh(Vector3[] _verts, Vector2[] _uvs, Vector3[] _normals, int[] _tris)
            {
                verts = new NativeArray<Vector3>(_verts, Allocator.TempJob);
                uvs = new NativeArray<Vector2>(_uvs, Allocator.TempJob);
                normals = new NativeArray<Vector3>(_normals, Allocator.TempJob);
                tris = new NativeArray<int>(_tris, Allocator.TempJob);

                zMin = -1;
                zMax = 1;
            }
            public NativeMesh(int vLen, int tLen)
            {
                verts = new NativeArray<Vector3>(vLen, Allocator.TempJob);
                uvs = new NativeArray<Vector2>(vLen, Allocator.TempJob);
                normals = new NativeArray<Vector3>(vLen, Allocator.TempJob);
                tris = new NativeArray<int>(tLen, Allocator.TempJob);

                zMin = -1;
                zMax = 1;
            }
            public void Dispose()
            {
                verts.Dispose();
                uvs.Dispose();
                normals.Dispose();
                tris.Dispose();
            }
        }

        // Struct to hold  start cap, middle segment, end cap meshs 
        public struct NativeMeshGroup : IDisposable
        {
            [NativeDisableParallelForRestriction]
            public NativeMesh start, mid, end;

            public NativeMeshGroup(Mesh s, Mesh m, Mesh e)
            {
                start = new NativeMesh(s);
                mid = new NativeMesh(m);
                end = new NativeMesh(e);
            }

            public void Dispose()
            {
                start.Dispose();
                mid.Dispose();
                end.Dispose();
            }

            //dont put in less than 3 segments plz
            public int GetTotalVerts(int segments)
            {
                return start.verts.Length + mid.verts.Length * (segments - 2) + end.verts.Length;
            }
            public int GetTotalTris(int segments)
            {
                return start.tris.Length + mid.tris.Length * (segments - 2) + end.tris.Length;
            }
        }

        // Struct to hold various parameters used in mesh generation
        public struct MeshGenParams
        {
            public float len;           // Total length of the path
            public float perSegment;    // Path-space distance of each segment
            public float uvperpath;     // Scale for belt UVs
            public float scaleFactor;   // Overall scale factor applied to generated mesh
            public int segments;        // Number of segments to use when generating 
            public bool beltUvs;        // Remap uvs for belts
        }



        public static float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }
}
