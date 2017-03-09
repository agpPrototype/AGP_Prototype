using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bond 
{
    public class BondManager : MonoBehaviour 
    {
		public static BondManager Instance = null;

        [SerializeField]
        private Slider BondBar;

		[SerializeField]
		[Range(0, 100)]
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
				UpdateGUI();

            }
        }

        // Use this for initialization
        void Awake () 
        {
            //initialize to be middle value
            //m_BondStatus = 50;

			//initialize the instance to this one
			if (Instance == null)
			{
				Instance = this;
			}
			else
			{
				if (Instance != this)
				{
					Destroy(this.gameObject);
				}
			}
			UpdateGUI();
		}   

        public void SetBondStatus(int b)
        {
            b = Mathf.Clamp(b, 0, 100);

            m_BondStatus = b;
            UpdateGUI();
        }

        public void UpdateGUI()
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
