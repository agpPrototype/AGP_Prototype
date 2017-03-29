using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameCritical;

namespace HealthCare
{
    public class AccaliaHealth : Health
    {

        protected override void OnDeathBegin()
        {
            if (m_Animator)
            {
                m_Animator.SetBool("Dead", true);
                m_Animator.SetTrigger("Death");
            }
            //showing death text
            GameController.Instance.RestartGameOnDeath("Accalia is dead! Restarting...");
        }

        public override void TakeDamage(float damage, GameObject dmgDealer = null)
        {
            base.TakeDamage(damage, dmgDealer);

            GetComponent<AI.CompanionAISM>().AgroAccalia(dmgDealer);
        }
    }
}
