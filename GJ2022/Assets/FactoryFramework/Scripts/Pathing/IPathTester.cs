using FactoryFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryFramework
{

    public class IPathTester : MonoBehaviour
    {
        public Vector3 start = Vector3.zero;
        public Vector3 end = Vector3.forward;
        public Vector3 startdir = Vector3.forward;
        public Vector3 enddir = Vector3.forward;
        public IPath p;
        public GlobalLogisticsSettings.PathSolveType pt = GlobalLogisticsSettings.PathSolveType.SPLINE;

        public BeltMeshSO frameBM;
        public BeltMeshSO beltBM;

        public MeshFilter frameFilter;
        public MeshFilter beltFilter;
        public void Regen()
        {
            p?.CleanUp();
            p = PathFactory.GeneratePathOfType(start, startdir, end, enddir, pt);
        }

        public void GenerateMesh()
        {
            frameFilter.mesh = BeltMeshGenerator.Generate(p, frameBM, ConveyorLogisticsUtils.settings.BELT_SEGMENTS_PER_UNIT * p.GetTotalLength(), 0.251f);
            beltFilter.mesh = BeltMeshGenerator.Generate(p, beltBM, ConveyorLogisticsUtils.settings.BELT_SEGMENTS_PER_UNIT * p.GetTotalLength(), 0.25f, 1f, true);
        }
    }
}
