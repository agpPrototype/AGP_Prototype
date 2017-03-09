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
        // Use this for initialization
        void Start()
        {
            m_CurrHP = MaxHP;
        }

        public void TakeDamage(float damage, GameObject dmgDealer = null)
        {
            m_CurrHP -= damage;
            if (m_CurrHP <= 0)
            {
                Destroy(gameObject);
            }
            else
            {
                StartCoroutine(Flash());                
            }

        }

        IEnumerator Flash()
        {
            Renderer render = GetComponent<Renderer>();
            Material m = render.material;
            Color32 c = render.material.color;
            render.material = null;
            render.material.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            render.material = m;
            render.material.color = c;
        }
    }
}
