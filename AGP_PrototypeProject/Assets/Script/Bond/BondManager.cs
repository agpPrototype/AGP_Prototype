using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bond 
{
    public class BondManager : MonoBehaviour 
    {
        [SerializeField]
        private Slider BondBar;

        private int m_BondStatus;

        public int BondStatus
        {
            get { return m_BondStatus; }

            // constrain between 0 and 100
            set
            {
                if (value > 100)
                {
                    m_BondStatus = 100;
                }
                else if (value < 0)
                {
                    m_BondStatus = 0;
                }
                else
                {
                    m_BondStatus = value;    
                }

            }
        }

        // Use this for initialization
        void Awake () 
        {
            //initialize to be middle value
            m_BondStatus = 50;
        }   

        public void SetBondStatus(int b)
        {
            b = Mathf.Clamp(b, 0, 100);

            m_BondStatus = b;
            UpdateGUI();
        }

        private void UpdateGUI()
        {
            if(!BondBar)
            {
                Debug.LogError("BondBar missing from scene");
                return;
            }
            BondBar.GetComponent<Slider>().value = m_BondStatus;
        }
    }

}
