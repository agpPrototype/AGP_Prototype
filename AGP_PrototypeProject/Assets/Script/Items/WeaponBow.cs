using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Items
{
    public class WeaponBow : EquipableItem {

        [SerializeField] 
        private GameObject Arrow;

        [SerializeField]
        private int NumArrows;

        [SerializeField]
        private Vector3 ArrowSpawnLocation;

        private GameObject m_CurrentArrow;

        private float m_ArrowForce;

        private bool m_IsPulling;


        // Use this for initialization
        void Awake () 
        {
            if(!Arrow)
            {
                Debug.LogError("Arrow GameObject missing");
            }
            m_IsPulling = false;
            m_ArrowForce = 0f;
        }

        // Update is called once per frame
        void Update () 
        {
            // accumulate force over time when the player is drawing the bow
            if(m_IsPulling)
            {
                m_ArrowForce += 1.0f;
            }
        }

        public override void DoActionBegin()
        {
            if(NumArrows > 0)
            {
                // fire an arrow
                m_CurrentArrow = Instantiate(Arrow, ArrowSpawnLocation, Quaternion.identity) as GameObject;
                SpendArrow();
                m_IsPulling = true;
            }
        }

        public override void DoActionRelease()
        {
            m_IsPulling = false;
            // get the forward of the player and apply multiply by force
            //arrow.GetComponent<ArrowComponent>().Initialize(Vector3 playerForward * m_ArrowForce);

            // reset force
            m_ArrowForce = 0.0f;
        }

        void AddArrow(int num)
        {
            NumArrows += num;
        }

        void SpendArrow()
        {
            NumArrows--;
        }
    }

}
