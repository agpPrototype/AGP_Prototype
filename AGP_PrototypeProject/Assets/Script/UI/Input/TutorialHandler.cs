using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using GameCritical;
using UI;

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
                if(m_TutorialPanel.GetIsActive())
                {
                    m_TutorialPanel.SlideOut();
                    m_TutorialPanel.AudioContainer.Play2DSound(0);
                    GameController.Instance.GameState = EnumService.GameState.InGame;
                }
            }
        }
    }
}
