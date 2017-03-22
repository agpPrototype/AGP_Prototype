using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * author: rob neir
 * date: 3/19/2017 
 * 
 * */

namespace Inputs
{
    public class UIControl : MonoBehaviour
    {
        private TutorialHandler m_TutorialHandler;

        private UserInput m_UserInput;
        private UIActions m_UIActions;

        // Use this for initialization
        void Start()
        {
            m_UserInput = UserInput.Instance;
            m_TutorialHandler = GetComponent<TutorialHandler>();
            m_UIActions = new UIActions();
            m_UIActions.InputPackets = new InputPacket[18];
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (m_UserInput)
            {
                m_UIActions.InputPackets = m_UserInput.InputPackets;
                ProcessUIInput();
            }
        }

        void ProcessUIInput()
        {
            m_TutorialHandler.ProcessTutorialActions(m_UIActions);
        }
    }
}
