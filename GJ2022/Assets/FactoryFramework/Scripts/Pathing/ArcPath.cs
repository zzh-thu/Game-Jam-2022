using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryFramework
{
    [System.Serializable]
    public class ArcPath : IPath
    {
        public ArcPathStruct mStruct;
        public ArcPath(Vector3 c, Vector3 s, Vector3 e, float rad, Vector3 forward, Vector3 norm)
        {
            mStruct.Initialize(c, s, e, rad, forward, norm);
        }
        ~ArcPath()
        {
            CleanUp();
        }
        public void CleanUp()
        {

        }

        public bool IsValid => mStruct.IsValid;
        public (Vector3, float) GetClosestPoint(Vector3 worldPoint) => mStruct.GetClosestPoint(worldPoint);
        public Vector3 GetDirectionAtPoint(float pathPercent) => mStruct.GetDirectionAtPoint(pathPercent);
        public Vector3 GetStart() => mStruct.GetStart();
        public Vector3 GetEnd() => mStruct.GetEnd();
        public (Vector3, Vector3, Vector3) GetPathVectors(float pathPercent) => mStruct.GetPathVectors(pathPercent);
        public Vector3 GetRightAtPoint(float pathPercent) => mStruct.GetRightAtPoint(pathPercent);
        public Quaternion GetRotationAtPoint(float pathPercent) => mStruct.GetRotationAtPoint(pathPercent);
        public float GetTotalLength() => mStruct.GetTotalLength();
        public Vector3 GetUpAtPoint(float pathPercent) => mStruct.GetUpAtPoint(pathPercent);
        public Vector3 GetWorldPointFromPathSpace(float pathPercent) => mStruct.GetWorldPointFromPathSpace(pathPercent);

    }

    public struct ArcPathStruct
    {
        public float radius;
        public float angle;
        public Vector3 center, start, end, normal;

        private bool _isValid;
        public bool IsValid => _isValid;
        public void Initialize(Vector3 c, Vector3 s, Vector3 e, float rad, Vector3 forward, Vector3 norm)
        {
            center = c;
            start = s;
            end = e;
            radius = rad;
            normal = norm;
            angle = CalculateArcAngFromPoints(c, forward, s, e, normal);
            CheckValid();
        }
        public void CleanUp()
        {

        }
        public bool CheckValid()
        {
            bool ret = true;
            if (start == end)
                ret = false;

            //more test cases?

            return ret;
        }
        public Vector3 GetEnd() => end;

        public Vector3 GetStart() => start;

        public Vector3 GetFrom() => (start - center).normalized;
        public float GetTotalLength() => radius * Mathf.Abs(angle);

        //there has to be a simpler way im just big dumb
        public (Vector3, float) GetClosestPoint(Vector3 worldPoint)
        {
            Vector3 closestIdealPoint = (worldPoint - center).normalized * radius + center;
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, normal);
            Vector3 rotStartDir = rot * (start - center).normalized;
            Vector3 rotEndDir = rot * (end - center).normalized;

            float startAng = Mathf.Atan2(rotStartDir.z, rotStartDir.x) + Mathf.PI;
            float endAng = Mathf.Atan2(rotEndDir.z, rotEndDir.x) + Mathf.PI;

            float p1, p2;
            p1 = Mathf.Min(startAng, endAng);
            p2 = Mathf.Max(startAng, endAng);

            Vector3 projectedIdealPointDir = rot * (closestIdealPoint - center).normalized;
            float idealAng = Mathf.Atan2(projectedIdealPointDir.z, projectedIdealPointDir.x) + Mathf.PI;
            bool onArc = (p2 - p1 <= Mathf.PI) == (p1 <= idealAng && idealAng <= p2);
            onArc = Mathf.Abs(angle) > Mathf.PI ? !onArc : onArc;

            if (!onArc)
            {
                float startDist = Vector3.Distance(start, worldPoint);
                float endDist = Vector3.Distance(end, worldPoint);

                closestIdealPoint = (startDist < endDist) ? start : end;
                if (startDist < endDist)
                    return (start, 0);
                else
                    return (end, 1);
            }
            //TODO this needs to be projected based on arc normal instead of always using z,x
            startAng = Mathf.Atan2(rotStartDir.z, rotStartDir.x);
            idealAng = Mathf.Atan2(projectedIdealPointDir.z, projectedIdealPointDir.x);
            float angDist = idealAng - startAng;
            if (angDist > Mathf.PI * 2)
                angDist -= Mathf.PI * 2;
            if (angDist < 0)
                angDist += Mathf.PI * 2;

            bool isRightArc = Vector3.Cross(rot * (end - start).normalized, rot * GetDirectionAtPoint(0)).y < 0;

            if (isRightArc)
                angDist = Mathf.PI * 2 - angDist;
            float pathP = Mathf.Abs(angDist / angle);
            return (GetWorldPointFromPathSpace(pathP), pathP);
        }

        public Vector3 GetWorldPointFromPathSpace(float pathPercent)
        {
            Vector3 startdir = (start - center).normalized;
            Vector3 enddir = (end - center).normalized;

            double ang = angle;

            double a = (Math.Abs(ang) - Math.PI);
            double b = (Math.PI - a);
            double c = Math.Abs(ang) / b;
            double mod = Math.Abs(ang) > Math.PI ? -1 * c : 1;

            float dot = Vector3.Dot(startdir, enddir);
            Vector3 relVec = (enddir - startdir * dot).normalized;

            float theta = Mathf.Acos(dot) * pathPercent * ((float)mod);
            Vector3 slerped = ((startdir * Mathf.Cos(theta)) + (relVec * Mathf.Sin(theta)));

            return slerped * radius + center;
        }

        private static float CalculateArcAngFromPoints(Vector3 center, Vector3 forward, Vector3 p1, Vector3 p2, Vector3 normal)
        {
            Vector3 v1 = (p1 - center).normalized;
            Vector3 v2 = (p2 - center).normalized;

            float cross = Vector3.Dot(Vector3.Cross(v1, v2), normal);
            float ang = Vector3.Angle(v1, v2) * Mathf.Deg2Rad;
            float mod = Mathf.Sign(cross);

            if (Vector3.Dot(v2, forward) < 0)
            {
                ang = Mathf.PI - ang + Mathf.PI;
                ang *= -1;
            }

            return ang * mod;
        }

        public Vector3 GetDirectionAtPoint(float pathPercent)
        {
            Vector3 tanPoint = GetWorldPointFromPathSpace(pathPercent);

            return Vector3.Cross((tanPoint - center).normalized, normal) * -1 * Mathf.Sign(angle);
        }

        public Vector3 GetRightAtPoint(float pathPercent)
        {
            Vector3 tanPoint = GetWorldPointFromPathSpace(pathPercent);
            Vector3 temp = Vector3.Cross(GetDirectionAtPoint(pathPercent), Vector3.up).normalized * -1;
            return temp;
        }

        public Vector3 GetUpAtPoint(float pathPercent)
        {
            return Vector3.Cross(GetDirectionAtPoint(pathPercent), GetRightAtPoint(pathPercent));
        }

        public (Vector3, Vector3, Vector3) GetPathVectors(float pathPercent)
        {
            throw new System.NotImplementedException();
        }
        public Quaternion GetRotationAtPoint(float pathPercent)
        {
            return Quaternion.LookRotation(GetDirectionAtPoint(pathPercent), GetUpAtPoint(pathPercent));
        }
    }
}
