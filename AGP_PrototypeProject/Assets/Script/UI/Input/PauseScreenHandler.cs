using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using UI;

namespace Inputs
{
    public class PauseScreenHandler : UIHandler
    {
        [SerializeField]
        private PauseMenu m_PauseMenu;

        void Start()
        {
            if (m_PauseMenu == null)
                Debug.LogError("Pause Menu is null in PauseScreenHandler");
            else
                m_PauseMenu.gameObject.SetActive(false);
        }

        public override void DoActions(UIActions uia)
        {
            if (uia.Start)
            {
                if (m_PauseMenu != null)
                {
                    m_PauseMenu.Show();
                }
            }
            if (uia.Back)
            {
                if (m_PauseMenu != null)
                {
                    m_PauseMenu.gameObject.SetActive(false);
                }
            }
        }
    }
}
