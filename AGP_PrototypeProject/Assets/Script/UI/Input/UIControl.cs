using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using System.Linq;

/*
 * author: rob neir
 * date: 3/19/2017 
 * 
 * */

namespace Inputs
{
    public class UIControl : MonoBehaviour
    {
        [SerializeField]
        private List<UIHandler> m_UIHandlers;

        private UserInput m_UserInput;
        private UIActions m_UIActions;

        // Use this for initialization
        void Start()
        {
            // fill UIHandlers with all handlers found.
            m_UIHandlers = new List<UIHandler>();
            m_UIHandlers = FindObjectsOfType<UIHandler>().ToList<UIHandler>();

            m_UserInput = UserInput.Instance;
            m_UIActions = new UIActions();
            m_UIActions.InputPackets = new InputPacket[18];
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (m_UserInput)
            {
                m_UIActions.InputPackets = m_UserInput.InputPackets;
                ProcessUIActions(m_UIActions);
                PropogateUIInput();
            }
        }

        public void ProcessUIActions(UIActions uia)
        {
            if (uia.InputPackets[(int)EnumService.InputType.O] != null)
            {
                uia.Back = Convert.ToBoolean(uia.InputPackets[(int)EnumService.InputType.O].Value);
            }
            if (uia.InputPackets[(int)EnumService.InputType.Command] != null)
            {
                uia.Start = Convert.ToBoolean(uia.InputPackets[(int)EnumService.InputType.Command].Value);
            }
        }

        // Propogates UI input to appropriate handlers.
        void PropogateUIInput()
        {
            // Go through all UIHandlers and give them input to process.
            for(int i = 0; i < m_UIHandlers.Count; i++)
            {
                UIHandler uiHandler = m_UIHandlers[i];
                if(uiHandler != null)
                {
                    uiHandler.DoActions(m_UIActions);
                }
            }
        }
    }
}
