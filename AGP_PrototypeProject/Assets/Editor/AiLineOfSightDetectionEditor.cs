using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using AI.Detection;

[ExecuteInEditMode]
[CustomEditor(typeof(AILineOfSightDetection))]
public class AiLineOfSightDetectionEditor : Editor {

    private AILineOfSightDetection instance;

    void OnEnable()
    {
        instance = (AILineOfSightDetection)target;
    }

    public override void OnInspectorGUI()
    {
        // BOX with field of view parameters.
        GUILayout.BeginVertical("box");

        GUILayout.Label("Field of view settings:");
        instance.SetPrivateFieldValue<float>("FOV", EditorGUILayout.Slider(new GUIContent("FOV", "Field of view of AI."),
            instance.GetPrivateFieldValue<float>("FOV"), 0, 360.0f));

        /////////////////////////////////////////////////////////////////////

        GUILayout.BeginVertical("box");
        GUILayout.Label("% breakup:");
        float directFOV = instance.GetPrivateFieldValue<float>("DirectFOVPercentage");
        float sideFOV = instance.GetPrivateFieldValue<float>("SideFOVPercentage");
        float peripheralFOV = instance.GetPrivateFieldValue<float>("PeripheralFOVPercentage");

        instance.SetPrivateFieldValue<float>("DirectFOVPercentage", EditorGUILayout.Slider(new GUIContent("DirectFOVPercentage", "This is the field of view easiest for the AI to see enemies."),
            directFOV, 0, 1.0f));
        instance.SetPrivateFieldValue<Color>("DirectFOVColor", EditorGUILayout.ColorField(new GUIContent("DirectFOVColor", "The color of the direct FOV debug lines."), 
            instance.GetPrivateFieldValue<Color>("DirectFOVColor")));

        instance.SetPrivateFieldValue<float>("SideFOVPercentage", EditorGUILayout.Slider(new GUIContent("SideFOVPercentage", "This is the field of view easiest for the AI to see enemies."),
            sideFOV, 0, 1.0f - directFOV));
        instance.SetPrivateFieldValue<Color>("SideFOVColor", EditorGUILayout.ColorField(new GUIContent("SideFOVColor", "The color of the side FOV debug lines."),
            instance.GetPrivateFieldValue<Color>("SideFOVColor")));

        instance.SetPrivateFieldValue<float>("PeripheralFOVPercentage", EditorGUILayout.Slider(new GUIContent("PeripheralFOVPercentage", "This is the field of view easiest for the AI to see enemies."),
            peripheralFOV, 0, 1.0f - directFOV - sideFOV));
        instance.SetPrivateFieldValue<Color>("PeripheralFOVColor", EditorGUILayout.ColorField(new GUIContent("PeripheralFOVColor", "The color of the peripheral FOV debug lines."), 
            instance.GetPrivateFieldValue<Color>("PeripheralFOVColor")));
        GUILayout.EndVertical();

        /////////////////////////////////////////////////////////////////////

        GUILayout.EndVertical();
        GUILayout.Space(20);

        base.OnInspectorGUI();
    }
}
