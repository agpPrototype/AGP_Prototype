using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bond 
{
    public class BondManager : MonoBehaviour 
    {
		public static BondManager Instance = null;

		private HashSet<BondEffect> m_RegisteredEffects;

		[SerializeField]
		[Range(0, 100)]
        private int m_BondStatus;

        public int BondStatus
        {
            get { return m_BondStatus; }

            // constrain between 0 and 100
            set
            {
				m_BondStatus = Mathf.Clamp(value, 0, 100);
				UpdateEffects();

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
				m_RegisteredEffects = new HashSet<BondEffect>();
			}
			else
			{
				if (Instance != this)
				{
					Destroy(this.gameObject);
				}
			}
			UpdateEffects();
		} 

		public void UpdateEffects()
		{
			foreach(BondEffect b in m_RegisteredEffects)
			{
				b.DoEffect();
			}
		}

		public void RegisterEffect(BondEffect be)
		{
			m_RegisteredEffects.Add(be);
		}

		public void UnRegisterEffect(BondEffect be)
		{
			m_RegisteredEffects.Remove(be);
		}
        
        // A function is needed to use in Behavior Tree
        public float GetBondStatus()
        {
            return (float)m_BondStatus;
        } 

		public float GetBondStatus01()
		{
			return (float)m_BondStatus / 100.0f;
		}

        public void SetBondStatus(int b)
        {
            b = Mathf.Clamp(b, 0, 100);

            m_BondStatus = b;
			UpdateEffects();
        }
    }

}
