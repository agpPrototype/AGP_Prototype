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
    private float m_minConeRadius = .01f;
    private int m_maxRaycasts = 8;
    private int m_minRaycasts = 1;
    private int m_maxCones = 3;
    private int m_minCones = 1;

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
        string coneRadiusStr = fovVar + "ConeRadius"; // Raycast distance string.
        string numRaycastsStr = fovVar + "NumbOfRaycasts"; // Raycast distance string.
        string numConesStr = fovVar + "NumbOfRings"; // Raycast distance string.
        // Values needed:
        float directFOV = instance.GetPrivateFieldValue<float>(fovVarStr);
        float raycastDist = instance.GetPrivateFieldValue<float>(raycastDistStr);
        float condRadius = instance.GetPrivateFieldValue<float>(coneRadiusStr);
        int numRaycasts = instance.GetPrivateFieldValue<int>(numRaycastsStr);
        int numCones = instance.GetPrivateFieldValue<int>(numConesStr);
        instance.SetPrivateFieldValue<float>(fovVarStr, EditorGUILayout.Slider(new GUIContent(fovVarStr, "Easy for the AI to see enemies within this view."),
            directFOV, 0, 1.0f));
        instance.SetPrivateFieldValue<Color>(colorStr, EditorGUILayout.ColorField(new GUIContent(colorStr, "The color of the direct FOV debug lines."),
            instance.GetPrivateFieldValue<Color>(colorStr)));
        instance.SetPrivateFieldValue<float>(raycastDistStr, EditorGUILayout.Slider(new GUIContent(raycastDistStr, "Easy for the AI to see enemies within this view."),
            raycastDist, 0, m_maxRaycastDistance));
        instance.SetPrivateFieldValue<int>(numRaycastsStr, (int)EditorGUILayout.Slider(new GUIContent(numRaycastsStr, "Easy for the AI to see enemies within this view."),
            numRaycasts, m_minRaycasts, m_maxRaycasts));
        if(numRaycasts > 1)
        {
            instance.SetPrivateFieldValue<float>(coneRadiusStr, EditorGUILayout.Slider(new GUIContent(coneRadiusStr, "Easy for the AI to see enemies within this view."),
                condRadius, m_minConeRadius, m_maxConeRadius));
            instance.SetPrivateFieldValue<int>(numConesStr, (int)EditorGUILayout.Slider(new GUIContent(numConesStr, "Easy for the AI to see enemies within this view."),
                numCones, m_minCones, m_maxCones));
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
        coneRadiusStr = fovVar + "ConeRadius"; // Raycast distance string.
        numRaycastsStr = fovVar + "NumbOfRaycasts"; // Raycast distance string.
        numConesStr = fovVar + "NumbOfRings"; // Raycast distance string.
        // Values needed:
        float sideFOV = instance.GetPrivateFieldValue<float>(fovVarStr);
        raycastDist = instance.GetPrivateFieldValue<float>(raycastDistStr);
        condRadius = instance.GetPrivateFieldValue<float>(coneRadiusStr);
        numRaycasts = instance.GetPrivateFieldValue<int>(numRaycastsStr);
        numCones = instance.GetPrivateFieldValue<int>(numConesStr);
        instance.SetPrivateFieldValue<float>(fovVarStr, EditorGUILayout.Slider(new GUIContent(fovVarStr, "Easy for the AI to see enemies within this view."),
            sideFOV, 0, 1.0f - directFOV));
        instance.SetPrivateFieldValue<Color>(colorStr, EditorGUILayout.ColorField(new GUIContent(colorStr, "The color of the direct FOV debug lines."),
            instance.GetPrivateFieldValue<Color>(colorStr)));
        instance.SetPrivateFieldValue<float>(raycastDistStr, EditorGUILayout.Slider(new GUIContent(raycastDistStr, "Easy for the AI to see enemies within this view."),
            raycastDist, 0, m_maxRaycastDistance));
        instance.SetPrivateFieldValue<int>(numRaycastsStr, (int)EditorGUILayout.Slider(new GUIContent(numRaycastsStr, "Easy for the AI to see enemies within this view."),
            numRaycasts, m_minRaycasts, m_maxRaycasts));
        if (numRaycasts > 1)
        {
            instance.SetPrivateFieldValue<float>(coneRadiusStr, EditorGUILayout.Slider(new GUIContent(coneRadiusStr, "Easy for the AI to see enemies within this view."),
                condRadius, m_minConeRadius, m_maxConeRadius));
            instance.SetPrivateFieldValue<int>(numConesStr, (int)EditorGUILayout.Slider(new GUIContent(numConesStr, "Easy for the AI to see enemies within this view."),
            numCones, m_minCones, m_maxCones));
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
        coneRadiusStr = fovVar + "ConeRadius"; // Raycast distance string.
        numRaycastsStr = fovVar + "NumbOfRaycasts"; // Raycast distance string.
        numConesStr = fovVar + "NumbOfRings"; // Raycast distance string.
        // Values needed:
        raycastDist = instance.GetPrivateFieldValue<float>(raycastDistStr);
        condRadius = instance.GetPrivateFieldValue<float>(coneRadiusStr);
        numRaycasts = instance.GetPrivateFieldValue<int>(numRaycastsStr);
        numCones = instance.GetPrivateFieldValue<int>(numConesStr);
        instance.SetPrivateFieldValue<float>(fovVarStr, EditorGUILayout.Slider(new GUIContent(fovVarStr, "Easy for the AI to see enemies within this view."),
            Mathf.Clamp(1.0f - directFOV - sideFOV, 0.0f, 1.0f), 0, 1.0f));
        instance.SetPrivateFieldValue<Color>(colorStr, EditorGUILayout.ColorField(new GUIContent(colorStr, "The color of the direct FOV debug lines."),
            instance.GetPrivateFieldValue<Color>(colorStr)));
        instance.SetPrivateFieldValue<float>(raycastDistStr, EditorGUILayout.Slider(new GUIContent(raycastDistStr, "Easy for the AI to see enemies within this view."),
            raycastDist, 0, m_maxRaycastDistance));
        instance.SetPrivateFieldValue<int>(numRaycastsStr, (int)EditorGUILayout.Slider(new GUIContent(numRaycastsStr, "Easy for the AI to see enemies within this view."),
            numRaycasts, m_minRaycasts, m_maxRaycasts));
        if (numRaycasts > 1)
        {
            instance.SetPrivateFieldValue<float>(coneRadiusStr, EditorGUILayout.Slider(new GUIContent(coneRadiusStr, "Easy for the AI to see enemies within this view."),
                condRadius, m_minConeRadius, m_maxConeRadius));
            instance.SetPrivateFieldValue<int>(numConesStr, (int)EditorGUILayout.Slider(new GUIContent(numConesStr, "Easy for the AI to see enemies within this view."),
            numCones, m_minCones, m_maxCones));
        }
        GUILayout.EndVertical();

        GUILayout.EndVertical(); 
        ///////////////////////////////////////////////////////////////////// END BOX

        GUILayout.Space(20);

        // draw normal inspector elements.
        base.OnInspectorGUI();

        // don't allow change values to revert to old values when "Play" is pressed in editor.
        if (GUI.changed) EditorUtility.SetDirty(target);
    }

    public void OnValidate()
    {

    }
}
