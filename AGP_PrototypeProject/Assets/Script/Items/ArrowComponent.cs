using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HealthCare;

namespace Items
{
    [RequireComponent( typeof(Rigidbody), typeof(BoxCollider) )]
    public class ArrowComponent : MonoBehaviour 
    {

        [SerializeField]
        private float LifeSpan = 2.0f;
        [SerializeField]
        private float Damage = 20.0f;
        public float ArrowDamage { get { return Damage; } }

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

        void OnTriggerEnter(Collider col)
        {

            //Debug.Log("COllision hit: " + col.gameObject.name);
            if (col.gameObject.GetComponent<Health>() && !col.gameObject.GetComponent<Player.PlayerControl>())
            {
                col.gameObject.GetComponent<Health>().TakeDamage(Damage, GameCritical.GameController.Instance.Player);
                GameCritical.GameController.Instance.Wolf.GetComponent<AI.CompanionAISM>().NotifyPlayerHitTarget(col.gameObject);
            }

            if (!col.gameObject.GetComponent<Player.PlayerControl>() && !col.gameObject.GetComponent<ActionZone>() && !col.gameObject.GetComponent<Misc.TutorialZone>())
            {
                GetComponent<MeshRenderer>().enabled = false;
                GetComponent<Rigidbody>().useGravity = false;
                GetComponent<Rigidbody>().velocity = Vector3.zero;
                GetComponent<Collider>().enabled = false;
                Time.timeScale = 1.0f;
                StartCoroutine(DelayDestroy());
                //Destroy(gameObject);
            }
        }

        IEnumerator DelayDestroy()
        {
            yield return new WaitForSeconds(1.5f);
            Destroy(gameObject);
        }
    } 
}

