using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

/*
 * author: rob neir
 * date: 3/19/2017 
 * 
 * */

namespace Inputs
{
    public class TutorialHandler : MonoBehaviour
    {
        [SerializeField]
        private TutorialPanel m_TutorialPanel;

        // Use this for initialization
        void Start()
        {
            if (m_TutorialPanel == null)
            {
                Debug.LogError("Tutorial panel null in tutorial handler.");
            }
        }

        public void ProcessTutorialActions(UIActions uia)
        {
            if (uia.InputPackets[(int)EnumService.InputType.O] != null)
            {
                uia.Back = Convert.ToBoolean(uia.InputPackets[(int)EnumService.InputType.O].Value);
            }

            DoActions(uia);
        }

        void DoActions(UIActions uia)
        {
            if(uia.Back)
            {
                m_TutorialPanel.SlideOut();
            }
        }
    }
}
