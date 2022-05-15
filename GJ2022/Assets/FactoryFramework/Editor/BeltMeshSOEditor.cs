using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using FactoryFramework;

[CustomEditor(typeof(BeltMeshSO))]
public class BeltMeshSOEditor : Editor
{
    UnityEditor.Editor startEditor;
    UnityEditor.Editor midEditor;
    UnityEditor.Editor endEditor;
    public override void OnInspectorGUI()
    {
        BeltMeshSO bmtarget = target as BeltMeshSO;
        bool changed = false;
        EditorGUI.BeginChangeCheck();

        bmtarget.basemesh = (Mesh)EditorGUILayout.ObjectField(bmtarget.basemesh, typeof(Mesh), true);

        if (EditorGUI.EndChangeCheck())
        {
            changed = true;
        }

        if (changed)
        {
            if (startEditor != null) DestroyImmediate(startEditor);
            if (midEditor != null) DestroyImmediate(midEditor);
            if (endEditor != null) DestroyImmediate(endEditor);

            bmtarget.CutBaseMesh();
            EditorUtility.SetDirty(target);
        }
        if (startEditor == null)
            startEditor = UnityEditor.Editor.CreateEditor(bmtarget.startCap.GetMesh());
        if (midEditor == null)
            midEditor = UnityEditor.Editor.CreateEditor(bmtarget.midSegment.GetMesh());
        if (endEditor == null)
            endEditor = UnityEditor.Editor.CreateEditor(bmtarget.endCap.GetMesh());
        GUIStyle g = new GUIStyle();
        GUILayout.Label("Start Cap");
        startEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(200, 200), g);
        GUILayout.Label("Mid Cap");
        midEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(200, 200), g);
        GUILayout.Label("End Cap");
        endEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(200, 200), g);


    }
}
