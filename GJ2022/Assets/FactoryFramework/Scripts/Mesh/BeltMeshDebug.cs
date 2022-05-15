using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryFramework
{
    public class BeltMeshDebug : MonoBehaviour
    {
        public BeltMeshSO bmso;
        public MeshFilter childStart, childMiddle, childEnd;
        // Start is called before the first frame update
        void Start()
        {

        }
        public void Slice()
        {
            bmso.CutBaseMesh();
            RenderMeshes();
        }
        public void RenderMeshes()
        {
            childStart.mesh = bmso.startCap.GetMesh();
            childMiddle.mesh = bmso.midSegment.GetMesh();
            childEnd.mesh = bmso.endCap.GetMesh();
        }

    }
}

