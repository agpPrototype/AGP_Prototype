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
        ///////////////////////////////////////////////////////////////////// START BOX

        GUILayout.BeginVertical("box"); 

        GUILayout.Label("Field of view settings:");
        instance.SetPrivateFieldValue<float>("FOV", EditorGUILayout.Slider(new GUIContent("FOV", "Field of view of AI."),
            instance.GetPrivateFieldValue<float>("FOV"), 0, 360.0f));

        ///////////////////////////////////////////////////////////////////// START BOX

        GUILayout.BeginVertical("box"); 

        GUILayout.Label("% breakup:");
        float directFOV = instance.GetPrivateFieldValue<float>("DirectFOVPercentage");
        float sideFOV = instance.GetPrivateFieldValue<float>("SideFOVPercentage");
        float peripheralFOV = instance.GetPrivateFieldValue<float>("PeripheralFOVPercentage");

        string fovVarStr = "DirectFOVPercentage"; // Name of fov variables.
        string colorStr = "DirectFOVColor"; // Name of color variable for debugging.
        instance.SetPrivateFieldValue<float>(fovVarStr, EditorGUILayout.Slider(new GUIContent(fovVarStr, "Easy for the AI to see enemies within this view."),
            directFOV, 0, 1.0f));
        instance.SetPrivateFieldValue<Color>(colorStr, EditorGUILayout.ColorField(new GUIContent(colorStr, "The color of the direct FOV debug lines."), 
            instance.GetPrivateFieldValue<Color>(colorStr)));

        fovVarStr = "SideFOVPercentage"; 
        colorStr = "SideFOVColor"; 
        instance.SetPrivateFieldValue<float>(fovVarStr, EditorGUILayout.Slider(new GUIContent(fovVarStr, "Medium difficulty for the AI to see enemies within this view."),
            sideFOV, 0, 1.0f - directFOV));
        instance.SetPrivateFieldValue<Color>(colorStr, EditorGUILayout.ColorField(new GUIContent(colorStr, "The color of the side FOV debug lines."),
            instance.GetPrivateFieldValue<Color>(colorStr)));

        fovVarStr = "PeripheralFOVPercentage";
        colorStr = "PeripheralFOVColor";
        instance.SetPrivateFieldValue<float>(fovVarStr, EditorGUILayout.Slider(new GUIContent(fovVarStr, "Hard for the AI to see enemies within this view."),
            1.0f - directFOV - sideFOV, 0, 1.0f - directFOV - sideFOV));
        instance.SetPrivateFieldValue<Color>(colorStr, EditorGUILayout.ColorField(new GUIContent(colorStr, "The color of the peripheral FOV debug lines."), 
            instance.GetPrivateFieldValue<Color>(colorStr)));

        GUILayout.EndVertical(); 

        ///////////////////////////////////////////////////////////////////// END BOX

        GUILayout.EndVertical(); 

        ///////////////////////////////////////////////////////////////////// END BOX

        GUILayout.Space(20);
        base.OnInspectorGUI();
    }
}
