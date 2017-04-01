using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class EndScreenCanvas : UIComponent
    {
        [SerializeField]
        private EndMenu m_EndMenu;
        public EndMenu EndMenu { get { return m_EndMenu; } }
    }
}
