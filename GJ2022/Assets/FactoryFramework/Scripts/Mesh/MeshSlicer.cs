using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace FactoryFramework
{
    public class MeshSlicer
    {
        public static (SerializableMesh, SerializableMesh) SliceAtZPos(Mesh m, float zpos)
        {
            int[] triangles = m.triangles;
            Vector3[] verts = m.vertices;
            Vector3[] normals = m.normals;
            Vector2[] uv0 = m.uv;

            TempMesh positive = new TempMesh();
            TempMesh negative = new TempMesh();

            //iterate over all triangles
            //separate into three cases 
            // - all verts positive side
            // - all verts negative side
            // - mixed

            for (int i = 0; i < triangles.Length; i += 3)
            {
                int indexa = triangles[i];
                int indexb = triangles[i + 1];
                int indexc = triangles[i + 2];
                Vector3 a = verts[indexa];
                Vector3 b = verts[indexb];
                Vector3 c = verts[indexc];

                bool aPositive = a.z > zpos;
                bool bPositive = b.z > zpos;
                bool cPositive = c.z > zpos;

                if (aPositive == bPositive && aPositive == cPositive)
                {
                    if (aPositive)
                    {
                        //Add to positive side mesh
                        positive.AddTriangleCopyFromMesh(m, new int[] { triangles[i], triangles[i + 1], triangles[i + 2] });
                        continue;
                    }
                    else
                    {
                        //Add to negative side mesh
                        negative.AddTriangleCopyFromMesh(m, new int[] { triangles[i], triangles[i + 1], triangles[i + 2] });
                        continue;
                    }
                }
                else
                {
                    //edges intersect cutting plane

                    Vector3 lonePoint;
                    if (aPositive != bPositive && bPositive == cPositive)
                        lonePoint = a;
                    else if (bPositive != aPositive && aPositive == cPositive)
                        lonePoint = b;
                    else
                        lonePoint = c;

                    //a-b b-c c-a
                    Vector3 ab = CalcZIntercept(a, b, zpos);
                    Vector3 bc = CalcZIntercept(b, c, zpos);
                    Vector3 ca = CalcZIntercept(c, a, zpos);

                    LinkedVertex av = new LinkedVertex(a, normals[indexa], uv0[indexa]);
                    LinkedVertex bv = new LinkedVertex(b, normals[indexb], uv0[indexb]);
                    LinkedVertex cv = new LinkedVertex(c, normals[indexc], uv0[indexc]);

                    if (lonePoint == a)
                    {
                        TempMesh target = aPositive ? positive : negative;
                        TempMesh offTarget = aPositive ? negative : positive;

                        LinkedVertex abv = CreateFromEdge(av, bv, zpos);
                        LinkedVertex cav = CreateFromEdge(cv, av, zpos);
                        target.AddRawTriangle(new LinkedVertex[] { av, abv, cav, });

                        offTarget.AddRawTriangle(new LinkedVertex[] { bv, cv, cav, });
                        offTarget.AddRawTriangle(new LinkedVertex[] { abv, bv, cav, });

                    }
                    else if (lonePoint == b)
                    {
                        TempMesh target = bPositive ? positive : negative;
                        TempMesh offTarget = bPositive ? negative : positive;

                        LinkedVertex bcv = CreateFromEdge(bv, cv, zpos);
                        LinkedVertex abv = CreateFromEdge(av, bv, zpos);
                        target.AddRawTriangle(new LinkedVertex[] { abv, bv, bcv, });

                        offTarget.AddRawTriangle(new LinkedVertex[] { av, abv, cv, });
                        offTarget.AddRawTriangle(new LinkedVertex[] { abv, bcv, cv, });
                    }
                    else
                    {
                        TempMesh target = cPositive ? positive : negative;
                        TempMesh offTarget = cPositive ? negative : positive;

                        LinkedVertex bcv = CreateFromEdge(bv, cv, zpos);
                        LinkedVertex cav = CreateFromEdge(cv, av, zpos);
                        target.AddRawTriangle(new LinkedVertex[] { cav, bcv, cv, });

                        offTarget.AddRawTriangle(new LinkedVertex[] { av, bv, cav, });
                        offTarget.AddRawTriangle(new LinkedVertex[] { bv, bcv, cav, });

                    }

                }
            }

            return (positive.ToSerializableMesh(), negative.ToSerializableMesh());

        }
        public static LinkedVertex CreateFromEdge(LinkedVertex a, LinkedVertex b, float zPos)
        {
            Vector3 intercept = CalcZIntercept(a.pos, b.pos, zPos);
            float totalDist = Vector3.Distance(a.pos, b.pos);
            float interceptDist = Vector3.Distance(a.pos, intercept);

            Vector2 uv = Vector2.Lerp(a.uv0, b.uv0, interceptDist / totalDist);
            Vector3 norm = Vector3.Slerp(a.normal, b.normal, interceptDist / totalDist); // I think this should be slerp?

            return new LinkedVertex(intercept, norm, uv);
        }
        public static Vector3 CalcZIntercept(Vector3 a, Vector3 b, float zPos)
        {
            Vector3 planeOrigin = new Vector3(0, 0, zPos);
            Vector3 planeNormal = new Vector3(0, 0, 1);
            Vector3 lineDir = (b - a).normalized;
            Vector3 lineOrigin = a;
            //line is origin + dir*dist

            //https://en.wikipedia.org/wiki/Line%E2%80%93plane_intersection
            float numerator = Vector3.Dot((planeOrigin - lineOrigin), planeNormal);
            float denominator = Vector3.Dot(lineDir, planeNormal);
            float dist = numerator / denominator;

            return lineOrigin + lineDir * dist;
        }
    }
    public class TempMesh
    {
        //very important that we only ever add to the end of this list
        public List<LinkedVertex> vertices = new List<LinkedVertex>();
        public List<int> triangles = new List<int>();

        public int GetIndexOfPosNorm(Vector3 pos, Vector3 norm)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                if (vertices[i].pos == pos && vertices[i].normal == norm)
                    return i;
            }
            return -1;
        }
        public void AddRawTriangle(LinkedVertex[] lva)
        {
            for (int i = 0; i < lva.Length; i++)
            {
                var lv = lva[i];
                int index = GetIndexOfPosNorm(lv.pos, lv.normal);
                if (index == -1)
                {
                    vertices.Add(lv);
                    index = vertices.Count - 1;
                }
                triangles.Add(index);
            }
        }
        public void AddTriangleCopyFromMesh(Mesh m, int[] indices)
        {
            for (int i = 0; i < indices.Length; i++)
            {
                int index = indices[i];
                LinkedVertex v = new LinkedVertex(m.vertices[index], m.normals[index], m.uv[index]);
                int localvertindex = GetIndexOfPosNorm(v.pos, v.normal);
                if (localvertindex == -1)
                {
                    vertices.Add(v);
                    localvertindex = vertices.Count - 1;
                }
                triangles.Add(localvertindex);
            }
        }
        public Mesh ToMesh()
        {
            Mesh m = new Mesh();
            m.vertices = vertices.Select<LinkedVertex, Vector3>((LinkedVertex v) => { return v.pos; }).ToArray();
            m.normals = vertices.Select<LinkedVertex, Vector3>((LinkedVertex v) => { return v.normal; }).ToArray();
            m.uv = vertices.Select<LinkedVertex, Vector2>((LinkedVertex v) => { return v.uv0; }).ToArray();

            m.triangles = triangles.ToArray();
            return m;
        }
        public SerializableMesh ToSerializableMesh()
        {
            SerializableMesh m = new SerializableMesh(ToMesh());
            return m;
        }
    }
    public class LinkedVertex
    {
        public Vector3 pos;
        public Vector3 normal;
        public Vector2 uv0;

        public LinkedVertex(Vector3 p, Vector3 n, Vector2 u0)
        {
            pos = p;
            normal = n;
            uv0 = u0;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            LinkedVertex other = obj as LinkedVertex;
            if (other == null)
                return false;

            return this.pos == other.pos && this.normal == other.normal;
        }

        //auto generated
        public override int GetHashCode()
        {
            int hashCode = 564474691;
            hashCode = hashCode * -1521134295 + pos.GetHashCode();
            hashCode = hashCode * -1521134295 + normal.GetHashCode();
            return hashCode;
        }
    }
}
