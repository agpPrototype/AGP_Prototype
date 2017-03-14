using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vfx
{
    [RequireComponent(typeof(SphereCollider))]
    public class ScanSphereDriver : MonoBehaviour
    {

        [SerializeField]
        private float m_ExpandSpeed = 0.2f;
        [SerializeField]
        private float m_StartRadius = 0.1f;
        [SerializeField]
        private float m_MaxRadius = 5.0f;
        [SerializeField]
        private bool m_IsLooping;

        private bool m_IsPlaying;
        private SphereCollider m_SphereCollider;

        // Use this for initialization
        void Start()
        {
            m_SphereCollider = this.GetComponent<SphereCollider>();
            // initialize sixe to start radius.
            this.transform.localScale = new Vector3(
                m_StartRadius,
                m_StartRadius,
                m_StartRadius);
            Play();
        }

        // Update is called once per frame
        void Update()
        {
            if(m_IsPlaying)
            {
                if (m_SphereCollider != null)
                {
                    if (this.transform.localScale.x < m_MaxRadius)
                    {
                        Vector3 currScale = transform.localScale;
                        float sizeIncreaseFactor = m_ExpandSpeed * Time.deltaTime;
                        this.transform.localScale = new Vector3(
                            currScale.x + sizeIncreaseFactor,
                            currScale.y + sizeIncreaseFactor,
                            currScale.z + sizeIncreaseFactor);
                    }
                    else
                    {
                        if (m_IsLooping)
                        {
                            this.transform.localScale = new Vector3(
                                m_StartRadius,
                                m_StartRadius,
                                m_StartRadius);
                        }
                    }
                }
            }
        }

        public void Play()
        {
            m_IsPlaying = true;
        }

        public void Stop()
        {
            m_IsPlaying = false;
        }

        public void SetIsLooping(bool isLooping)
        {
            m_IsLooping = isLooping;
        }
    }
}