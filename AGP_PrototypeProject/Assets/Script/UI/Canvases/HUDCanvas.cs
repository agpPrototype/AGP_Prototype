using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inputs;
using UnityEngine.UI;

namespace UI
{
    public class HUDCanvas : UIComponent
    {
        [SerializeField]
        private Text DeathText;

        [SerializeField]
        private BondSliderEffect m_BondBar;
        public BondSliderEffect BondBar { get { return m_BondBar; } }

        void Start()
        {
            if (DeathText)
            {
                DeathText.enabled = false;
            }
        }

        public void ShowDeathText(string text)
        {
            DeathText.enabled = true;
            DeathText.text = text;
        }
    }
}
