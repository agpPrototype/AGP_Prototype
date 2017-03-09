using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inputs;
using Utility;
using System;
using Player;
using Items;

namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(MoveComponent))]
    public class PlayerControl : MonoBehaviour
    {

        [SerializeField]
        private UserInput m_UserInput;

        [SerializeField]
        private Transform m_cam;

        private MoveComponent m_moveComp;
        //private InputPacket[] m_inputPackets;
        private EquipmentHandler m_EquipmentHandler;

        private PCActions m_PCActions;

        void Start()
        {
            if (Camera.main)
            {
                m_cam = Camera.main.transform;
            }
            else
            {
                Debug.Log("SCENE MSISING CAMERA");
            }
            m_moveComp = GetComponent<MoveComponent>();
            m_EquipmentHandler = GetComponent<EquipmentHandler>();
            m_PCActions = new PCActions();
            m_PCActions.InputPackets = new InputPacket[18];
            m_UserInput = FindObjectOfType<UserInput>();
        }

        void FixedUpdate()
        {

            if (m_UserInput)
            {
                m_PCActions.InputPackets = m_UserInput.InputPackets;
                ProcessInput();
            }
        }

        void ProcessInput()
        {
            CheckMovement();
            CheckActions();
            CheckCommands();
        }

        private void CheckCommands()
        {

        }

        private void CheckActions()
        {
            m_EquipmentHandler.ProcessActions(m_PCActions);
        }

        private void CheckMovement()
        {
            m_moveComp.ProcessMovement(m_PCActions);
        }
    }
}
