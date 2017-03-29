using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameCritical;

namespace HealthCare
{
    public class PlayerHealth : Health
    {
        protected override void OnDeathBegin()
        {
            if (m_Animator)
            {
                m_Animator.SetBool("Dead", true);                              
            }
            //showing death text
            GameController.Instance.RestartGameOnDeath("Player is dead! Restarting...");
        }

        public override void TakeDamage(float damage, GameObject dmgDealer = null)
        {
            base.TakeDamage(damage, dmgDealer);

            GameCritical.GameController.Instance.Wolf.GetComponent<AI.CompanionAISM>().SetMainState(AI.WolfMainState.Attack);
        }

    }
}
