using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI.Detection;
using AI;

namespace vfx
{
    [RequireComponent(typeof(MeshRenderer))]
    public class AudioPulse : MonoBehaviour
    {
        [SerializeField]
        private float m_Interval;
        private float m_IntervalTimer;

        [SerializeField]
        private float m_DangerRange;
        private float m_DangerRangeSquared;

        private Camera m_MainCamera;
        private ScannerEffectDriver m_ScannerEffectDriver;
        private bool m_Enabled;

        // Use this for initialization
        void Start()
        {
            m_IntervalTimer = m_Interval;
            m_DangerRangeSquared = m_DangerRange * m_DangerRange;
            m_MainCamera = Camera.main;
            m_Enabled = true;
            if (m_MainCamera != null)
            {
                m_ScannerEffectDriver = m_MainCamera.GetComponent<ScannerEffectDriver>();
            }
        }

        public void Enable()
        {
            m_Enabled = true;
        }

        public void Disable()
        {
            m_Enabled = false;
            m_ScannerEffectDriver.Stop();
        }

        // Update is called once per frame
        void Update()
        {
            if (m_Enabled)
            {
                m_IntervalTimer += Time.deltaTime;
                if (m_IntervalTimer >= m_Interval)
                {
                    // go through enemies and find out if they are close.
                    List<GameObject> enemies = GameCritical.GameController.Instance.GetAllEnemies();
                    bool enemyClose = false;
                    for (int i = 0; i < enemies.Count; i++)
                    {
                        GameObject enemy = enemies[i];
                        if (enemy != null)
                        {
                            float distSquared = (this.transform.position - enemy.transform.position).sqrMagnitude;
                            if (distSquared <= m_DangerRangeSquared)
                            {
                                // If an enemy has found player or something then don't play audio pulse.
                                EnemyAISM enemyAISM = enemy.GetComponent<EnemyAISM>();
                                if (enemyAISM.CurrentState != EnemyAISM.EnemyAIState.CHASING &&
                                    enemyAISM.CurrentState != EnemyAISM.EnemyAIState.ATTACKING &&
                                    enemyAISM.CurrentState != EnemyAISM.EnemyAIState.LOOKING)
                                {
                                    enemyClose = true;
                                    break;
                                }
                            }
                        }
                    }

                    // play the scanner effect if close to enemy.
                    if (m_ScannerEffectDriver != null)
                    {
                        if (enemyClose)
                        {
                            m_IntervalTimer = 0;
                            m_ScannerEffectDriver.Play();
                        }
                        else
                        {
                            m_ScannerEffectDriver.Stop();
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Scanner driver is null.");
                    }
                }
            }
        }
    }
}