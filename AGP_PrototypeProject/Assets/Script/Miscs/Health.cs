using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Misc
{
    public class Health : MonoBehaviour
    {
        [SerializeField]
        private float MaxHP = 100;

        private float m_CurrHP;
        private Animator m_Animator;
        // Use this for initialization
        void Start()
        {
            m_CurrHP = MaxHP;
            m_Animator = GetComponent<Animator>();
        }

        public void TakeDamage(float damage, GameObject dmgDealer = null)
        {
            if (m_CurrHP > 0)
            {
                m_CurrHP -= damage;
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

        virtual protected void OnDeathBegin()
        {
            if (m_Animator)
            {
                m_Animator.SetBool("Dead", true);
                Destroy(gameObject, 2.0f);
            }
        }
    }
}
