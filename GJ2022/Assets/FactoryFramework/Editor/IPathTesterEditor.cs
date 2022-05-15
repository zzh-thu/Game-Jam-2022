using FactoryFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(IPathTester))]
public class IPathTesterEditor : Editor
{
    IPathTester holder;
    void OnEnable()
    {
        holder = (IPathTester)target;
        if (holder.p == null)
        {
            holder.Regen();
        }
    }
    void OnSceneGUI()
    {
        //Input();
        Draw();
    }

    public float pathP = 0.5f;
    public Vector3 closestPosTarget = new Vector3(0, 1, 0);

    void Draw()
    {
        bool changed = false;

        //draw anchors
        Handles.color = Color.white;
        Vector3 newStartPos = Handles.FreeMoveHandle(holder.start, Quaternion.identity, 0.1f, Vector3.zero, Handles.CircleHandleCap);
        if (newStartPos != holder.start)
        {
            holder.start = newStartPos;
            changed = true;
        }

        Handles.color = Color.black;
        Vector3 newEndPos = Handles.FreeMoveHandle(holder.end, Quaternion.identity, 0.1f, Vector3.zero, Handles.CircleHandleCap);
        if (newEndPos != holder.end)
        {
            holder.end = newEndPos;
            changed = true;
        }

        Handles.color = Color.yellow;
        Handles.DrawLine(holder.start, holder.start + holder.startdir * 0.2f);
        Quaternion newStartAng = Handles.Disc(Quaternion.LookRotation(holder.startdir, Vector3.up), holder.start, Vector3.up, 0.3f, false, 5);
        Vector3 newForward = newStartAng * Vector3.forward;
        if (newForward != holder.startdir)
        {
            holder.startdir = newForward;
            changed = true;
        }

        Handles.DrawLine(holder.end, holder.end + holder.enddir * 0.2f);
        Quaternion newEndAng = Handles.Disc(Quaternion.LookRotation(holder.enddir, Vector3.up), holder.end, Vector3.up, 0.3f, false, 5);
        Vector3 newEndForward = newEndAng * Vector3.forward;
        if (newEndForward != holder.enddir)
        {
            holder.enddir = newEndForward;
            changed = true;
        }

        Vector3 pMid = holder.p.GetWorldPointFromPathSpace(pathP);
        Handles.color = Color.red; //right
        Handles.DrawLine(pMid, pMid + holder.p.GetRightAtPoint(pathP) * 0.2f, 4f);

        Handles.color = Color.cyan; // up
        Handles.DrawLine(pMid, pMid + holder.p.GetUpAtPoint(pathP) * 0.2f, 4f);

        Handles.color = Color.green; // forward
        Handles.DrawLine(pMid, pMid + holder.p.GetDirectionAtPoint(pathP) * 0.2f, 4f);

        if (holder.p as SmartPath != null)
        {
            SmartPath bs = holder.p as SmartPath;
            var (a, b) = bs.subPaths[0];
            foreach (var (subP, _) in bs.subPaths)
            {
                if (subP.GetType() == typeof(ArcPath))
                {
                    ArcPath arc = subP as ArcPath;
                    Handles.color = Color.blue;
                    Handles.DrawWireArc(arc.mStruct.center, arc.mStruct.normal, arc.mStruct.GetFrom(), arc.mStruct.angle * Mathf.Rad2Deg, arc.mStruct.radius, 2);
                }
                else if (subP.GetType() == typeof(SegmentPath))
                {
                    Handles.color = Color.green;
                    SegmentPath segment = subP as SegmentPath;
                    Handles.DrawLine(segment.GetStart(), segment.GetEnd(), 2);
                }
            }
        }
        else if (holder.p as CubicBezierPath != null)
        {
            CubicBezierPath cbp = holder.p as CubicBezierPath;
            Handles.DrawBezier(cbp.mStruct.start, cbp.mStruct.end, cbp.mStruct.controlPointA, cbp.mStruct.controlPointB, Color.blue, null, 2f);
        }
        else if (holder.p as SegmentPath != null)
        {
            SegmentPath sp = holder.p as SegmentPath;
            Handles.color = Color.blue;
            Handles.DrawLine(sp.mStruct.start, sp.mStruct.end, 2f);
        }

        //Draw closestPoint holder
        Handles.color = Color.yellow;
        closestPosTarget = Handles.FreeMoveHandle(closestPosTarget, Quaternion.identity, 0.1f, Vector3.zero, Handles.CircleHandleCap);
        var closest = holder.p.GetClosestPoint(closestPosTarget);
        Handles.FreeMoveHandle(closest.Item1, Quaternion.identity, 0.05f, Vector3.zero, Handles.CircleHandleCap);

        if (changed)
            holder.Regen();

    }

    public override void OnInspectorGUI()
    {
        bool changed = false;

        float pathPercent = EditorGUILayout.Slider(pathP, 0, 1);
        if (pathPercent != pathP)
        {
            pathP = pathPercent;
            changed = true;
        }

        GlobalLogisticsSettings.PathSolveType newType = (GlobalLogisticsSettings.PathSolveType)EditorGUILayout.EnumPopup(holder.pt);
        if (newType != holder.pt)
        {
            holder.pt = newType;
            changed = true;
        }

        EditorGUILayout.LabelField("Length: " + holder.p?.GetTotalLength());

        if (changed)
        {
            holder.Regen();
            HandleUtility.Repaint();
            SceneView.RepaintAll();
        }

        holder.frameBM = (BeltMeshSO)EditorGUILayout.ObjectField(holder.frameBM, typeof(BeltMeshSO), true);
        holder.beltBM = (BeltMeshSO)EditorGUILayout.ObjectField(holder.beltBM, typeof(BeltMeshSO), true);
        holder.frameFilter = (MeshFilter)EditorGUILayout.ObjectField(holder.frameFilter, typeof(MeshFilter), true);
        holder.beltFilter = (MeshFilter)EditorGUILayout.ObjectField(holder.beltFilter, typeof(MeshFilter), true);

        if (GUILayout.Button("Generate mesh"))
        {
            holder.GenerateMesh();
        }
    }
}
