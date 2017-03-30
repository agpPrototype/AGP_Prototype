using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI.Detection;

namespace vfx
{
    [RequireComponent(typeof(MeshRenderer))]
    public class AudioPulseDriver : MonoBehaviour
    {

        [SerializeField]
        private Material m_Material;
        [SerializeField]
        [Range(1.0f, float.MaxValue)]
        private float m_ExpandPercentage;
        [SerializeField]
        private float m_StartRadius = 0.1f;
        [SerializeField]
        private float m_MaxRadius = 5.0f;
        [SerializeField]
        private float m_Interval = 2.6f;
        [SerializeField]
        private float m_DangerRange = 15.0f;
        private float m_DangerRangeSquared;

        [SerializeField]
        private bool m_IsLooping;
        private bool m_IsInTimeInterval;

        private bool m_IsPlaying;
        private MeshRenderer m_MeshRenderer;

        // Use this for initialization
        void Start()
        {
            m_MeshRenderer = GetComponent<MeshRenderer>();

            m_DangerRangeSquared = m_DangerRange * m_DangerRange;

            // initialize sixe to start radius.
            this.transform.localScale = new Vector3(
                m_StartRadius,
                m_StartRadius,
                m_StartRadius);
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_IsInTimeInterval)
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
                            enemyClose = true;
                            break;
                        }
                    }
                }

                // if enemy is close turn on the pulse.
                if (enemyClose)
                {
                    if (!m_IsPlaying)
                    {
                        Play();
                    }
                }
                else
                {
                    Stop();
                }

                // make sphere expand.
                if (m_IsPlaying)
                {
                    if (this.transform.localScale.x < m_MaxRadius)
                    {
                        Vector3 currScale = transform.localScale;
                        m_MeshRenderer.enabled = true;
                        m_IsInTimeInterval = false;
                        this.transform.localScale = new Vector3(
                            currScale.x * m_ExpandPercentage,
                            currScale.y * m_ExpandPercentage,
                            currScale.z * m_ExpandPercentage);
                    }
                    else
                    {
                        m_MeshRenderer.enabled = false;
                        if (m_IsLooping)
                        {
                            m_IsInTimeInterval = true;
                            StartCoroutine(WaitForInterval());
                        }
                    }
                }
            }
        }

        private IEnumerator WaitForInterval()
        {
            yield return new WaitForSeconds(m_Interval);
            Play();
        }

        public void Play()
        {
            this.transform.localScale = new Vector3(
                m_StartRadius,
                m_StartRadius,
                m_StartRadius);
            m_IsInTimeInterval = false;
            m_IsPlaying = true;
        }

        public void Stop()
        {
            this.transform.localScale = new Vector3(
                m_StartRadius,
                m_StartRadius,
                m_StartRadius);
            m_IsPlaying = false;
        }

        public void SetIsLooping(bool isLooping)
        {
            m_IsLooping = isLooping;
        }
    }
}