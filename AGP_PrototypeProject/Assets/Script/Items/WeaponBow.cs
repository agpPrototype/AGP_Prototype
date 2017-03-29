using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;
using Audio;


namespace Items
{
    [RequireComponent(typeof(AudioContainer))]
    public class WeaponBow : EquipableItem {

        [SerializeField] 
        private GameObject Arrow;

        [SerializeField]
        private int NumArrows;

        [SerializeField]
        private Transform ArrowSpawnLocation;

        private GameObject m_CurrentArrow;

        private float m_ArrowForce;

        private bool m_IsPulling;

        private float m_FireDelay = 0.0f;

        private AudioContainer m_AudioContainer;


        // Use this for initialization
        void Awake () 
        {
            if(!Arrow)
            {
                Debug.LogError("Arrow GameObject missing");
            }
            m_IsPulling = false;
            m_ArrowForce = 0f;
        }

        void Start()
        {
            m_AudioContainer = GetComponent<AudioContainer>();
        }

        // Update is called once per frame
        void Update () 
        {
            // accumulate force over time when the player is drawing the bow
            if(m_IsPulling)
            {
                m_ArrowForce += 1.0f;
            }
            m_FireDelay += Time.deltaTime;
            ////ray hit point
            //Vector3 rayHitPoint = Vector3.zero;
            //Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
            //Vector3 worldSpace = Vector3.zero;
            //RaycastHit arrowHit;
            //if (Physics.Raycast(ray, out arrowHit))
            //{
            //    worldSpace = arrowHit.point;
            //    rayHitPoint = worldSpace;
            //}

            //Vector3 connectVec = Vector3.zero;
            //if (rayHitPoint != Vector3.zero)
            //{
            //    connectVec = rayHitPoint - ArrowSpawnLocation.position;
            //}
            //else
            //{
            //    Vector3 ininiteHitPoint = Camera.main.transform.position + ray.direction * 1000.0f;
            //    connectVec = ininiteHitPoint - ArrowSpawnLocation.position;
            //}
            //Debug.DrawRay(ArrowSpawnLocation.position, connectVec, Color.red);
            //Debug.DrawRay(ArrowSpawnLocation.position, ray.direction, Color.yellow);
        }

        public override void DoAction()
        {
            DoActionBegin();
            DoActionRelease();
        }

        public override void DoActionBegin()
        {
            if(NumArrows > 0 && m_FireDelay > 0.8f)
            {
                m_FireDelay = 0.0f;
                // fire an arrow
                m_CurrentArrow = Instantiate(Arrow, ArrowSpawnLocation.position, GetComponentInParent<PlayerControl>().transform.rotation) as GameObject;
                float horizontalAmmount = (Screen.width / 2.0f) / Camera.main.pixelWidth;
                float verticalAmmount = (Screen.height / 2.0f) / Camera.main.pixelHeight;

                m_AudioContainer.PlaySound(1); // play shoot arrow sound.

                //ray hit point
                Vector3 rayHitPoint = Vector3.zero;
                Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
                Vector3 worldSpace = Vector3.zero;
                RaycastHit arrowHit;
                if (Physics.Raycast(ray, out arrowHit))
                {
                    // Bypass the collider of 1 Action Zone so that you can call "MoveTo" from outside of an Action Zone
                    if (arrowHit.collider.gameObject.GetComponent<ActionZone>() || arrowHit.collider.gameObject.GetComponent<Misc.TutorialZone>())
                    {
                        ray = new Ray(arrowHit.point, ray.direction);
                        Physics.Raycast(ray, out arrowHit);

                    }

                    worldSpace = arrowHit.point;
                    rayHitPoint = worldSpace;
                }

                Vector3 connectVec = Vector3.zero;
                if (rayHitPoint != Vector3.zero)
                {
                    connectVec = rayHitPoint - ArrowSpawnLocation.position;
                }
                else
                {
                    Vector3 ininiteHitPoint = Camera.main.transform.position + ray.direction * 1000.0f;
                    connectVec = ininiteHitPoint - ArrowSpawnLocation.position;
                }
                connectVec.Normalize();

                m_CurrentArrow.GetComponent<Rigidbody>().velocity = connectVec * 30.0f;
                SpendArrow();
                m_IsPulling = true;
                m_CurrentArrow.GetComponent<ArrowComponent>().Initialize();

                if (ActivateKillCam(arrowHit.collider.gameObject, m_CurrentArrow.GetComponent<ArrowComponent>()))
                {
                    Time.timeScale = 0.45f;
                    GameCritical.GameController.Instance.Player.GetComponent<MoveComponent>().m_CamRig.Target = m_CurrentArrow.transform;
                }
            }
        }

        public override void DoActionRelease()
        {
            m_IsPulling = false;
            // get the forward of the player and apply multiply by force
            //arrow.GetComponent<ArrowComponent>().Initialize(Vector3 playerForward * m_ArrowForce);

            // reset force
            m_ArrowForce = 0.0f;
        }

        private bool ActivateKillCam(GameObject hitObject, ArrowComponent arrow)
        {
            if (hitObject && hitObject.GetComponent<AI.EnemyAISM>())
            {
                // play killcam noise
                float enemyHP = hitObject.GetComponent<HealthCare.Health>().CurrentHP;
                if(enemyHP - arrow.ArrowDamage <= 0)
                {
                    if(GameCritical.GameController.Instance.BondManager.BondStatus > 50.0f)
                    {
                        m_AudioContainer.PlaySound(2);
                        return true;
                    }
                }
            }

            return false;
        }

        void AddArrow(int num)
        {
            NumArrows += num;
        }

        void SpendArrow()
        {
            NumArrows--;
        }
    }

}
