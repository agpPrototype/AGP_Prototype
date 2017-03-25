using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inputs;
using Utility;
using System;
using Player;
using Items;
using AI;
using GameCritical;

namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(MoveComponent))]
    public class PlayerControl : MonoBehaviour
    {

        [SerializeField]
        private Transform m_cam;

        private MoveComponent m_moveComp;
        //private InputPacket[] m_inputPackets;
        private EquipmentHandler m_EquipmentHandler;
        private PowerHandler m_PowerHandler;
        private CommandHandler m_CommandHandler;
        private UserInput m_UserInput;

        private PCActions m_PCActions;
        private CompanionAISM m_WolfAI;

        public bool BellRing;
        private bool wasBellingRing;

        void Start()
        {
            if (Camera.main)
            {
                m_cam = Camera.main.transform;
            }
            else
            {
                Debug.Log("SCENE MISSING CAMERA");
            }
            m_moveComp = GetComponent<MoveComponent>();
            m_EquipmentHandler = GetComponent<EquipmentHandler>();
            m_PowerHandler = GetComponent<PowerHandler>();
            m_CommandHandler = GetComponent<CommandHandler>();
            m_PCActions = new PCActions();
            m_PCActions.InputPackets = new InputPacket[18];
            m_UserInput = UserInput.Instance;
            m_WolfAI = FindObjectOfType<CompanionAISM>();
            Initialize();

            GameController.Instance.RegisterPlayer(gameObject);
        }

        void Initialize()
        {
            if (m_WolfAI)
            {
                if (m_CommandHandler)
                {

                }
            }
        }

        void Update()
        {
            if (BellRing && GameController.Instance.GameState != EnumService.GameState.Win_BellRing)
            {
                GameController.Instance.GameState = EnumService.GameState.Win_BellRing;
            }
        }

        void FixedUpdate()
        {
            if (m_UserInput && GameController.Instance.GameState == EnumService.GameState.InGame)
            {
                m_PCActions.InputPackets = m_UserInput.InputPackets;
                ProcessInput();
            } else if (GameController.Instance.GameState == EnumService.GameState.Win_BellRing)
            {
                DoEndGame();
            }
        }

        void ProcessInput()
        {
            CheckMovement();
            CheckActions();
            CheckCommands();
            CheckPowers();
        }

        private void CheckCommands()
        {
            m_CommandHandler.ProcessCommands(m_PCActions);
        }

        private void CheckActions()
        {
            m_EquipmentHandler.ProcessActions(m_PCActions);
        }

        private void CheckMovement()
        {
            m_moveComp.ProcessMovement(m_PCActions);
        }

        private void CheckPowers()
        {
            m_PowerHandler.ProcessPowers(m_PCActions);
        }

        private void DoEndGame()
        {
            m_moveComp.DoEndGame();
            m_WolfAI.DoEndGame();
        }

        
    }
}