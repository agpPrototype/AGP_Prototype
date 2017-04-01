using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI;

namespace Misc
{
    public class AccaliaTutorialZone : TutorialZone
    {
        private CompanionAISM m_AISM;
        private Animator m_Animator;

        // Use this for initialization
        void Start()
        {
            m_AISM = FindObjectOfType<CompanionAISM>();
            m_Animator = m_AISM.GetComponent<Animator>();
            if(m_Animator != null)
            {
                m_Animator.speed = 0;
            }            
        }

        public override void OnTutorialZoneEnter(Collider col)
        {
            base.OnTutorialZoneEnter(col);

            if (m_Animator != null)
            {
                m_Animator.speed = 1;
            }
        }
    }
}
