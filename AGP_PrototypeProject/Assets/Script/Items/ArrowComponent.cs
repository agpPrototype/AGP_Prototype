using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Misc;

namespace Items
{
    [RequireComponent( typeof(Rigidbody), typeof(BoxCollider) )]
    public class ArrowComponent : MonoBehaviour 
    {

        [SerializeField]
        private float LifeSpan = 2.0f;
        [SerializeField]
        private float Damage = 20.0f;

        private Rigidbody m_Rigidbody;

        // Use this for initialization
        void Start () 
        {
            m_Rigidbody = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update () 
        {

        }

        public void Initialize()
        {
            //m_Rigidbody.AddForce(force, ForceMode.Impulse);

            // remove yourself after LifeSpan seconds to keep object count down
            Destroy(gameObject, LifeSpan);
        }

        void OnCollisionEnter(Collision col)
        {
            if (col.gameObject.GetComponent<Health>())
            {
                col.gameObject.GetComponent<Health>().TakeDamage(Damage);
            }
        }
    }
}

