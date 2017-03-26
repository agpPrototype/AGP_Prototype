using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class PauseScreenCanvas : MonoBehaviour
    {
        [SerializeField]
        private PauseMenu m_PauseMenu;
        public PauseMenu PauseMenu { get { return m_PauseMenu; } }
    }
}
