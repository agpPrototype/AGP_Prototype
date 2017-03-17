using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HealthCare
{
    public abstract class Health : MonoBehaviour
    {
        [SerializeField]
        protected float MaxHP = 100;

        [SerializeField]
        private Image m_HealthBar;

        protected float m_CurrHP;
        protected Animator m_Animator;
        // Use this for initialization
        void Start()
        {
            Initialize();
        }

        protected virtual void Initialize()
        {
            m_CurrHP = MaxHP;
            m_Animator = GetComponent<Animator>();
            UpdateHealthBar();
        }

        private void UpdateHealthBar()
        {
            if (m_HealthBar != null)
            {
                m_HealthBar.fillAmount = m_CurrHP / MaxHP;
            }
        }

        public virtual void TakeDamage(float damage, GameObject dmgDealer = null)
        {
            if (m_CurrHP > 0)
            {
                m_CurrHP -= damage;
                UpdateHealthBar();
                if (m_CurrHP <= 0)
                {
                    OnDeathBegin();
                }
                else
                {
                    StartCoroutine(Flash());
                }
            }

        }

        IEnumerator Flash()
        {
            Renderer render = GetComponentInChildren<Renderer>();

            //Material m = render.material;
            //Color32 c = render.material.color;
            //render.material = null;
            //render.material.color = Color.red;
            //yield return new WaitForSeconds(0.2f);
            //render.material = m;
            //render.material.color = c;
            for (int i = 0; i < 3; i++)
            {
                render.enabled = false;
                yield return new WaitForSeconds(0.1f);
                render.enabled = true;
                yield return new WaitForSeconds(0.1f);
            }
        }

        protected abstract void OnDeathBegin();
    }
}
