using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// author: Rob Neir
/// </summary>
/// 

namespace UI
{
    public class UIComponent : MonoBehaviour
    {
        [SerializeField]
        private bool m_IsActive = false;
        public bool GetIsActive() { return m_IsActive; }
        private void SetIsActive(bool active)
        {
            m_IsActive = active;
        }

        public virtual void Activate()
        {
            SetIsActive(true);
        }

        public virtual void Deactivate()
        {
            SetIsActive(false);
        }
    }
}
