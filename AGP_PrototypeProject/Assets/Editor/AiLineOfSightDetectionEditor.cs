using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using AI.Detection;

[ExecuteInEditMode]
[CustomEditor(typeof(AILineOfSightDetection))]
public class AiLineOfSightDetectionEditor : Editor {

    private AILineOfSightDetection instance;

    private float m_maxRaycastDistance = 300.0f;
    private float m_maxConeRadius = 6.0f;
    private int m_maxRaycasts = 8;
    private int m_maxCones = 3;

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

        GUILayout.Label("Direct FOV:");
        GUILayout.BeginVertical("box");
        string fovVar = "Direct";
        // Strings needed:
        string fovVarStr = fovVar + "FOVPercentage"; // Name of fov variables.
        string colorStr = fovVar + "FOVColor"; // Name of color variable for debugging.
        string raycastDistStr = fovVar + "RaycastMaxDistance"; // Raycast distance string.
        string condRadiusStr = fovVar + "ConeRadius"; // Raycast distance string.
        string numRaycastsStr = fovVar + "NumbOfRaycasts"; // Raycast distance string.
        string numConesStr = fovVar + "NumbOfRings"; // Raycast distance string.
        // Values needed:
        float directFOV = instance.GetPrivateFieldValue<float>(fovVarStr);
        float raycastDist = instance.GetPrivateFieldValue<float>(raycastDistStr);
        float condRadius = instance.GetPrivateFieldValue<float>(condRadiusStr);
        int numRaycasts = instance.GetPrivateFieldValue<int>(numRaycastsStr);
        int numCones = instance.GetPrivateFieldValue<int>(numConesStr);
        instance.SetPrivateFieldValue<float>(fovVarStr, EditorGUILayout.Slider(new GUIContent(fovVarStr, "Easy for the AI to see enemies within this view."),
            directFOV, 0, 1.0f));
        instance.SetPrivateFieldValue<Color>(colorStr, EditorGUILayout.ColorField(new GUIContent(colorStr, "The color of the direct FOV debug lines."),
            instance.GetPrivateFieldValue<Color>(colorStr)));
        instance.SetPrivateFieldValue<float>(raycastDistStr, EditorGUILayout.Slider(new GUIContent(raycastDistStr, "Easy for the AI to see enemies within this view."),
            raycastDist, 0, m_maxRaycastDistance));
        instance.SetPrivateFieldValue<float>(condRadiusStr, EditorGUILayout.Slider(new GUIContent(condRadiusStr, "Easy for the AI to see enemies within this view."),
            condRadius, 0, m_maxConeRadius));
        instance.SetPrivateFieldValue<int>(numRaycastsStr, (int)EditorGUILayout.Slider(new GUIContent(numRaycastsStr, "Easy for the AI to see enemies within this view."),
            numRaycasts, 1, m_maxRaycasts));
        if(numRaycasts > 1)
        {
            instance.SetPrivateFieldValue<int>(numConesStr, (int)EditorGUILayout.Slider(new GUIContent(numConesStr, "Easy for the AI to see enemies within this view."),
                numCones, 1, m_maxCones));
        }
        GUILayout.EndVertical();
        
        EditorGUILayout.Separator();

        GUILayout.Label("Side FOV:");
        GUILayout.BeginVertical("box");
        fovVar = "Side";
        // Strings needed:
        fovVarStr = fovVar + "FOVPercentage"; // Name of fov variables.
        colorStr = fovVar + "FOVColor"; // Name of color variable for debugging.
        raycastDistStr = fovVar + "RaycastMaxDistance"; // Raycast distance string.
        condRadiusStr = fovVar + "ConeRadius"; // Raycast distance string.
        numRaycastsStr = fovVar + "NumbOfRaycasts"; // Raycast distance string.
        numConesStr = fovVar + "NumbOfRings"; // Raycast distance string.
        // Values needed:
        float sideFOV = instance.GetPrivateFieldValue<float>(fovVarStr);
        raycastDist = instance.GetPrivateFieldValue<float>(raycastDistStr);
        condRadius = instance.GetPrivateFieldValue<float>(condRadiusStr);
        numRaycasts = instance.GetPrivateFieldValue<int>(numRaycastsStr);
        numCones = instance.GetPrivateFieldValue<int>(numConesStr);
        instance.SetPrivateFieldValue<float>(fovVarStr, EditorGUILayout.Slider(new GUIContent(fovVarStr, "Easy for the AI to see enemies within this view."),
            sideFOV, 0, 1.0f - directFOV));
        instance.SetPrivateFieldValue<Color>(colorStr, EditorGUILayout.ColorField(new GUIContent(colorStr, "The color of the direct FOV debug lines."),
            instance.GetPrivateFieldValue<Color>(colorStr)));
        instance.SetPrivateFieldValue<float>(raycastDistStr, EditorGUILayout.Slider(new GUIContent(raycastDistStr, "Easy for the AI to see enemies within this view."),
            raycastDist, 0, m_maxRaycastDistance));
        instance.SetPrivateFieldValue<float>(condRadiusStr, EditorGUILayout.Slider(new GUIContent(condRadiusStr, "Easy for the AI to see enemies within this view."),
            condRadius, 0, m_maxConeRadius));
        instance.SetPrivateFieldValue<int>(numRaycastsStr, (int)EditorGUILayout.Slider(new GUIContent(numRaycastsStr, "Easy for the AI to see enemies within this view."),
            numRaycasts, 1, m_maxRaycasts));
        if (numRaycasts > 1)
        {
            instance.SetPrivateFieldValue<int>(numConesStr, (int)EditorGUILayout.Slider(new GUIContent(numConesStr, "Easy for the AI to see enemies within this view."),
            numCones, 1, m_maxCones));
        }
        GUILayout.EndVertical();

        EditorGUILayout.Separator();

        GUILayout.Label("Peripheral FOV:");
        GUILayout.BeginVertical("box");
        fovVar = "Periph";
        // Strings needed:
        fovVarStr = fovVar + "FOVPercentage"; // Name of fov variables.
        colorStr = fovVar + "FOVColor"; // Name of color variable for debugging.
        raycastDistStr = fovVar + "RaycastMaxDistance"; // Raycast distance string.
        condRadiusStr = fovVar + "ConeRadius"; // Raycast distance string.
        numRaycastsStr = fovVar + "NumbOfRaycasts"; // Raycast distance string.
        numConesStr = fovVar + "NumbOfRings"; // Raycast distance string.
        // Values needed:
        raycastDist = instance.GetPrivateFieldValue<float>(raycastDistStr);
        condRadius = instance.GetPrivateFieldValue<float>(condRadiusStr);
        numRaycasts = instance.GetPrivateFieldValue<int>(numRaycastsStr);
        numCones = instance.GetPrivateFieldValue<int>(numConesStr);
        instance.SetPrivateFieldValue<float>(fovVarStr, EditorGUILayout.Slider(new GUIContent(fovVarStr, "Easy for the AI to see enemies within this view."),
            1.0f - directFOV - sideFOV, 0, 1.0f));
        instance.SetPrivateFieldValue<Color>(colorStr, EditorGUILayout.ColorField(new GUIContent(colorStr, "The color of the direct FOV debug lines."),
            instance.GetPrivateFieldValue<Color>(colorStr)));
        instance.SetPrivateFieldValue<float>(raycastDistStr, EditorGUILayout.Slider(new GUIContent(raycastDistStr, "Easy for the AI to see enemies within this view."),
            raycastDist, 0, m_maxRaycastDistance));
        instance.SetPrivateFieldValue<float>(condRadiusStr, EditorGUILayout.Slider(new GUIContent(condRadiusStr, "Easy for the AI to see enemies within this view."),
            condRadius, 0, m_maxConeRadius));
        instance.SetPrivateFieldValue<int>(numRaycastsStr, (int)EditorGUILayout.Slider(new GUIContent(numRaycastsStr, "Easy for the AI to see enemies within this view."),
            numRaycasts, 1, m_maxRaycasts));
        if (numRaycasts > 1)
        {
            instance.SetPrivateFieldValue<int>(numConesStr, (int)EditorGUILayout.Slider(new GUIContent(numConesStr, "Easy for the AI to see enemies within this view."),
            numCones, 1, m_maxCones));
        }
        GUILayout.EndVertical();

        GUILayout.EndVertical(); 
        ///////////////////////////////////////////////////////////////////// END BOX

        GUILayout.Space(20);
        base.OnInspectorGUI();
    }

    public void OnValidate()
    {

    }
}
