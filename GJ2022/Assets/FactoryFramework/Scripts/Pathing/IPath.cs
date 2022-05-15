using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

namespace FactoryFramework
{
    public interface IPath
    {
        public bool IsValid { get; }
        public float GetTotalLength();
        public (Vector3, float) GetClosestPoint(Vector3 worldPoint);

        public Vector3 GetWorldPointFromPathSpace(float pathPercent);

        public Vector3 GetStart();

        public Vector3 GetEnd();

        public Vector3 GetDirectionAtPoint(float pathPercent);
        public Vector3 GetRightAtPoint(float pathPercent);
        public Vector3 GetUpAtPoint(float pathPercent);

        //Forward, Right, Up
        public (Vector3, Vector3, Vector3) GetPathVectors(float pathPercent);

        public Quaternion GetRotationAtPoint(float pathPercent);
        public void CleanUp();
    }

    // This interface designates that an object supports Jobs based mesh generation
    public interface IPathMeshGenerator
    {
        // Create a MeshGenConfig<pathstruct> job, then return the handle to it
        public JobHandle RunMeshGenJob(ref BeltMeshGenerator.NativeMeshGroup inputMesh, ref BeltMeshGenerator.NativeMesh outputMesh, ref BeltMeshGenerator.MeshGenParams settings);
    }
}