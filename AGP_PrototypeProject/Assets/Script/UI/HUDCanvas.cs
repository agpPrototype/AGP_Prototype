using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inputs;

namespace UI
{
    public class HUDCanvas : MonoBehaviour
    {
        [SerializeField]
        private BondSliderEffect m_BondBar;
        public BondSliderEffect BondBar { get { return m_BondBar; } }
    }
}
