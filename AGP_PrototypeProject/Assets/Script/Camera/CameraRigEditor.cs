using UnityEngine;
using UnityEditor;
using System.Collections;
using CameraController;

namespace CameraController
{
    [CustomEditor(typeof(CameraRig))]
    public class CameraRigEditor : Editor
    {

        CameraRig cameraRig;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            cameraRig = (CameraRig)target;

            EditorGUILayout.LabelField("Camera Helper");

            if (GUILayout.Button("Save camera's position"))
            {
                Camera cam = Camera.main;
                if (cam)
                {
                    Transform camT = cam.transform;
                    Vector3 camPos = camT.localPosition;
                    Vector3 camRight = camPos;
                    Vector3 camLeft = camPos;
                    camLeft.x = -camPos.x;
                    cameraRig.CameraSetting.CamPositionOffsetRight = camRight;
                    cameraRig.CameraSetting.CamPositionOffsetLeft = camLeft;
                }
            }
        }
    }
}
