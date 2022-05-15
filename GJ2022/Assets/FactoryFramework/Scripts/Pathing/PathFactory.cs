using FactoryFramework;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryFramework
{
    public static class PathFactory
    {
        public static IPath GeneratePath(Vector3 start, Vector3 startDir, Vector3 end, Vector3 endDir)
        {
            var settings = ConveyorLogisticsUtils.settings;
            var pt = settings.PATHTYPE;
            return GeneratePathOfType(start, startDir, end, endDir, pt);
        }
        public static IPath GeneratePathOfType(Vector3 start, Vector3 startDir, Vector3 end, Vector3 endDir, GlobalLogisticsSettings.PathSolveType pt)
        {
            var settings = ConveyorLogisticsUtils.settings;

            if (pt == GlobalLogisticsSettings.PathSolveType.SMART)
            {
                return new SmartPath(start, startDir, end, endDir, settings.BELT_TURN_RADIUS, settings.BELT_VERTICAL_TOLERANCE, settings.BELT_RAMP_RADIUS);
            }
            else if (pt == GlobalLogisticsSettings.PathSolveType.SPLINE)
            {
                return new CubicBezierPath(start, end, startDir, endDir, settings.BELT_TURN_RADIUS);
            }
            else if (pt == GlobalLogisticsSettings.PathSolveType.SEGMENT)
            {
                return new SegmentPath(start, startDir, end, endDir);
            }
            return null;
        }

        public static bool CollisionAlongPath(IPath p, float resolution, float radius, LayerMask layermask, Collider[] ignored = null, int startskip = 0, int endskip = 0)
        {
            var settings = ConveyorLogisticsUtils.settings;
            int maxColliders = 1;
            int checks = Mathf.Max(0, Mathf.FloorToInt(p.GetTotalLength() / resolution));
            for (int i = startskip; i < checks - endskip; i++)
            {
                float pathPos = ((float)i + 0.5f) / checks;
                Vector3 pos = p.GetWorldPointFromPathSpace(pathPos);
                Collider[] hitColliders = new Collider[maxColliders];
                Physics.OverlapSphereNonAlloc(pos, radius, hitColliders, layermask);
                foreach (Collider collider in hitColliders)
                {
                    if (collider == null) continue;
                    if (ignored != null && !ignored.Contains(collider))
                    {
                        //if (settings.SHOW_DEBUG_LOGS)
                        //    Debug.Log("colliding with " + collider + " at position " + collider.gameObject.transform.position);
                        return true;
                    }
                    else if (ignored == null)
                    {
                        //if (settings.SHOW_DEBUG_LOGS)
                        //    Debug.Log("colliding with " + collider);
                        return true;
                    }

                }
            }
            return false;
        }
    }
}

