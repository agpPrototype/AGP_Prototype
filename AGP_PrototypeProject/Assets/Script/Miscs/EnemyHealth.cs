using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Audio;

namespace HealthCare
{
    [RequireComponent(typeof(AudioContainer))]
    public class EnemyHealth : Health
    {
        private AudioContainer m_AudioContainer;

        private Dictionary<GameObject, float> m_DamagerToDamage; // map of damager to the amount of damage they gave.

        protected override void Initialize()
        {
            base.Initialize();
        }

        void Start()
        {
            Initialize();
            m_AudioContainer = GetComponent<AudioContainer>();
        }

        protected override void OnDeathBegin()
        {
            // Remove this enemy from the zone's list
            gameObject.GetComponent<AI.EnemyAISM>().MyActionZone.EnemyDestroyed(gameObject);

            if (m_Animator)
            {
                if (m_AudioContainer)
                {
                    m_AudioContainer.PlaySound(3);
                }
                m_Animator.SetBool("Dead", true);
                Destroy(gameObject, 2.0f);
            }
        }

        public override void TakeDamage(float damage, GameObject dmgDealer = null)
        {
            base.TakeDamage(damage, dmgDealer);
            AI.EnemyAISM enemyAI = GetComponent<AI.EnemyAISM>();

            if (enemyAI)
            {
                if(m_AudioContainer)
                {
                    m_AudioContainer.PlaySound(3);
                }
                if(!(enemyAI.CurrentState == AI.EnemyAISM.EnemyAIState.ATTACKING || enemyAI.CurrentState == AI.EnemyAISM.EnemyAIState.CHASING))
                {
                    if (dmgDealer.GetComponent<AI.Detection.AIVisible>())
                        enemyAI.AgroToTarget(dmgDealer.GetComponent<AI.Detection.AIVisible>());
                }
            }
        }
    }
}
