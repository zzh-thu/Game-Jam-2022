using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace FactoryFramework
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "BeltMesh", menuName = "Factory Framework/BeltMesh")]
    public class BeltMeshSO : ScriptableObject
    {
        public Mesh basemesh;
        [SerializeField, HideInInspector]
        public SerializableMesh startCap;
        [SerializeField, HideInInspector]
        public SerializableMesh endCap;
        [SerializeField, HideInInspector]
        public SerializableMesh midSegment;

        //public float cuttingPoint;

        public void CutBaseMesh()
        {
            float cuttingPoint = basemesh.bounds.extents.z / 3;
            SerializableMesh temp;
            startCap = null;
            endCap = null;
            midSegment = null;
            (endCap, temp) = MeshSlicer.SliceAtZPos(basemesh, cuttingPoint);
            (midSegment, startCap) = MeshSlicer.SliceAtZPos(temp.GetMesh(), -1 * cuttingPoint);
        }
    }
}