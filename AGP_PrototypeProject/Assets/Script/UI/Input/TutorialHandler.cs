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
    public class TutorialHandler : UIHandler
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

        public override void DoActions(UIActions uia)
        {
            if (uia.Back)
            {
                m_TutorialPanel.SlideOut();
            }
        }
    }
}
