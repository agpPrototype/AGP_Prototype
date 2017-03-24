using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        }

        public override void TakeDamage(float damage, GameObject dmgDealer = null)
        {
            base.TakeDamage(damage, dmgDealer);

            GameCritical.GameController.Instance.Wolf.GetComponent<AI.CompanionAISM>().SetMainState(AI.WolfMainState.Attack);
        }

    }
}
