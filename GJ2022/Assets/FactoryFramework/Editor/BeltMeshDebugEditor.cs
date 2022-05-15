using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using FactoryFramework;

[CustomEditor(typeof(BeltMeshDebug))]
public class BeltMeshDebugEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Slice"))
        {
            ((BeltMeshDebug)target).Slice();
        }

    }
}


