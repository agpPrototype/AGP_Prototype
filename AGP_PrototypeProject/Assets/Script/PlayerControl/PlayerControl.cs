using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inputs;
using Utility;
using System;

public class PlayerControl : MonoBehaviour {

    [SerializeField]
    private UserInput m_UserInput;

    [SerializeField]
    private Transform m_cam;

    private MoveComponent m_moveComp;
    private InputPacket[] m_inputPackets;

    [System.Serializable]
    private class PCActions
    {
        public float Horizontal;
        public float Vertical;
        public Vector3 Move;
        public Vector3 CamForward;
        public Vector3 CamRight;

    }

    private PCActions m_PCActions;

	void Update()
    {
        
        if (m_UserInput)
        {
            m_inputPackets = m_UserInput.InputPackets;
            ProcessInput();
        }
    }

    void ProcessInput()
    {
        CheckMovement();    
        CheckActions();
        CheckCommands();
        CheckCamera();
    }

    private void CheckCamera()
    {
        throw new NotImplementedException();
    }

    private void CheckCommands()
    {
        throw new NotImplementedException();
    }

    private void CheckActions()
    {
        throw new NotImplementedException();
    }

    private void CheckMovement()
    {
        float h = m_inputPackets[(int)EnumService.InputType.LeftStickY].Value;
        float v = m_inputPackets[(int)EnumService.InputType.LeftStickX].Value;

        m_PCActions.CamForward = Vector3.Scale(m_cam.forward, new Vector3(1, 0, 1)).normalized;
        m_PCActions.CamRight = m_cam.right;

        m_PCActions.Move = v * m_PCActions.CamForward + h * m_PCActions.CamRight;


    }
}
