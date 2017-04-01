using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inputs;

namespace UI
{
    public class TutorialCanvas : UIComponent
    {
        [SerializeField]
        private TutorialPanel m_TutorialPanel;
        public TutorialPanel TutorialPanel { get { return m_TutorialPanel; } }
    }
}
