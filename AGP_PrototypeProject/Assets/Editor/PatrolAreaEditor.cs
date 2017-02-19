using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AI;

[CustomEditor(typeof(PatrolArea))]
public class PatrolAreaEditor : Editor
{
    private PatrolArea instance;

    void OnEnable()
    {
        instance = (PatrolArea)target;
    }

    public override void OnInspectorGUI()
    {
        if(GUILayout.Button("Add Waypoint"))
        {
            instance.InstantiateWaypoint();
        }
        EditorGUILayout.Separator();
        EditorGUILayout.BeginVertical("box");
        base.OnInspectorGUI();
        EditorGUILayout.EndVertical();
    }
}