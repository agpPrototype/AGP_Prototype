using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioSourceLoudnessTester : MonoBehaviour
    {
        [Tooltip("Amount of time until we update the clip loudness. We can change this number to make sampling happen less often.")]
        [SerializeField]
        public float UpdateStep = 0.1f;

        [Tooltip("How much data we are sampling from clip.")]
        [SerializeField]
        private int SampleDataLength = 512;

        private float m_CurrentUpdateTime = 0f;

        private float m_ClipLoudness;
        public float ClipLoudness
        {
            get
            {
                return m_ClipLoudness;
            }
        }

        private float[] m_ClipSampleData;

        private AudioSource m_AudioSource;

        // Use this for initialization
        void Awake()
        {
            m_AudioSource = this.GetComponent<AudioSource>();
            if (!m_AudioSource)
            {
                Debug.LogError(GetType() + ".Awake: there was no audioSource set.");
            }
            m_ClipSampleData = new float[SampleDataLength];
        }

        // Update is called once per frame
        void Update()
        {
            m_CurrentUpdateTime += Time.deltaTime;
            if (m_CurrentUpdateTime >= UpdateStep)
            {
                m_CurrentUpdateTime = 0f;
                if (m_AudioSource.clip != null)
                {
                    m_AudioSource.clip.GetData(m_ClipSampleData, m_AudioSource.timeSamples); //I read 1024 samples, which is about 80 ms on a 44khz stereo clip, beginning at the current sample position of the clip.
                    m_ClipLoudness = 0f;
                    foreach (var sample in m_ClipSampleData)
                    {
                        m_ClipLoudness += Mathf.Abs(sample);
                    }
                    m_ClipLoudness /= SampleDataLength; //clipLoudness is what you are looking for
                }
            }

        }
    }
}