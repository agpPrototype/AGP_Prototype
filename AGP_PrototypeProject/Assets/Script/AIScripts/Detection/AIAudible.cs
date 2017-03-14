using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// AUTHOR: rob neir
/// 
/// DESCRIPTION: when this component is attached to a game object then that game object
/// has the ability to be heard by AI. 
/// 
/// </summary>
namespace AI
{
    namespace Detection
    {
        [RequireComponent(typeof(AudioSource))]
        public class AIAudible : AIDetectable
        {
            #region Member Variables

            [SerializeField]
            [Tooltip("How loud this AIAudible source is which determines area of effect.")]
            private float m_Range = 10.0f;
            public float Range { get { return m_Range;  } }

            [SerializeField]
            [Tooltip("How much this audio gets dampened when travelling through objects. This number will be multiplied by range of audio when hitting an object.")]
            private float m_Dampen = 0.5f;

            [Tooltip("Should we draw the sphere of effect area of this sound.")]
            [SerializeField]
            private bool m_IsDrawSphereOfEffect;

            [Tooltip("Should we draw the line that represents the effect of this sound.")]
            [SerializeField]
            private bool m_IsDrawLineOfEffect;

            [Tooltip("Destroy the audio source if it is not playing. This means if it does not play on awake it will be destroyed.")]
            [SerializeField]
            private bool m_IsDestroyIfNotPlaying = false;

            private AudioSource m_AudioSource;

            /* event for when audible is spawned to notify DetectionManager */
            public delegate void Audible_Spawn_EventHandler(AIAudible audible);
            public static event Audible_Spawn_EventHandler AudibleSpawnEvt;

            /* event for when audible is destroyed to notify DetectionManager */
            public delegate void Audible_Destroy_EventHandler(AIAudible audible);
            public static event Audible_Destroy_EventHandler AudibleDestroyEvt;

            #endregion

            protected override void Start()
            {
                base.Start();
                m_AudioSource = GetComponent<AudioSource>();
            }

            private void Update()
            {
                // if we want to destroy the audio source after playing it then do so.
                if(m_IsDestroyIfNotPlaying)
                {
                    if(m_AudioSource)
                    {
                        if(!m_AudioSource.isPlaying)
                        {
                            Destroy(gameObject);
                        }
                    }
                }
            }

            public override void RegisterToDetectionManager()
            {
                /* send event to DetectionManager about spawning this audible. */
                if (AudibleSpawnEvt != null)
                {
                    AudibleSpawnEvt(this);
                }
            }

            public override void OnDestroy()
            {
                /* send event to DetectionManager about destroying this audible. */
                if (AudibleDestroyEvt != null)
                {
                    AudibleDestroyEvt(this);
                }
            }
            
            public bool RangeCheckToGameObject(GameObject targetGO, GameObject startGO, Vector3 targetPos,  Vector3 startPos, float range)
            {
                // disable starting collider so raycast doesn't hit object it starts in.
                Collider startingGOCollider = startGO.GetComponent<Collider>();
                if(startingGOCollider != null)
                {
                    startingGOCollider.enabled = false;
                }

                RaycastHit rayHit;
                Vector3 dir = targetPos - startPos;
                bool isHit = Physics.Raycast(startPos, dir, out rayHit, range);

                // re-enable collider on starting game object.
                if (startingGOCollider != null)
                {
                    startingGOCollider.enabled = true;
                }

                if (isHit)
                {
                    // determine what to do based on result of raycast.
                    GameObject gObject = rayHit.collider.gameObject;
                    if(gObject == null)
                    {
#if UNITY_EDITOR
                        if(m_IsDrawLineOfEffect)
                            Debug.DrawLine(startPos, targetPos, Color.white, 2.0f);
#endif
                        return false;
                    }
                    else if(gObject == targetGO)
                    {
#if UNITY_EDITOR
                        if (m_IsDrawLineOfEffect)
                            Debug.DrawLine(startPos, rayHit.point, Color.magenta / 2.0f, 2.0f);
#endif
                        return true;
                    }
                    else
                    {
#if UNITY_EDITOR
                        if (m_IsDrawLineOfEffect)
                            Debug.DrawLine(startPos, rayHit.point, Color.blue, 2.0f);
#endif
                        Vector3 newStartPos = rayHit.point;
                        float newRange = range * m_Dampen;
                        return RangeCheckToGameObject(targetGO, gObject, targetPos, newStartPos, newRange);
                    }
                }

                // if we got this far we did not hit the target.
                return false;
            }

#if UNITY_EDITOR 
            public void OnDrawGizmos()
            {
                if(m_IsDrawSphereOfEffect)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(this.transform.position, m_Range);
                }
            }
#endif
        }; // AIAudible class
    }; // Detection namespace
}; // AI namespace
