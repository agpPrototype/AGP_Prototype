using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI;
using AI.Detection;

namespace Audio
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(AudioSourceLoudnessTester))]
    public class AudioSourceDetectable : MonoBehaviour
    {
        [Tooltip("If false the sphere of affect range will change size with respect to the audio clip volume.")]
        [SerializeField]
        private bool IsFixedAudibleDistance = false;

        [SerializeField]
        [Tooltip("It is multiplied by the volume of audio source to determine sphere collider size if IsFixedAudibleDistance is false.")]
        private float AudioStrength = 1.0f;

        [Tooltip("Color of audio source volume trigger for debug.")]
        [SerializeField]
        private Color SphereVolumeColor;

        [Tooltip("Should we draw audio source sphere trigger..")]
        [SerializeField]
        private bool IsDrawSphereVolume;

        private AudioSource m_AudioSource;
        private SphereCollider m_SphereCollider;
        private AudioSourceLoudnessTester m_LoudnessTester;
        private float m_Loudness; // Combination of ClipLoudness (from loudness tester) * AudioStrength (specified by this script)

        void Awake()
        {
            m_AudioSource = this.GetComponent<AudioSource>();
            m_SphereCollider = this.GetComponent<SphereCollider>();
            m_LoudnessTester = this.GetComponent<AudioSourceLoudnessTester>();
        }

        // Update is called once per frame
        void Update()
        {
            if (IsFixedAudibleDistance)
            {
                return;
            }

            // Update loudness and set sphere size.
            m_Loudness = m_LoudnessTester.ClipLoudness * AudioStrength;
            m_SphereCollider.radius = m_Loudness;
        }

        public float GetLoudness()
        {
            return m_LoudnessTester.ClipLoudness;
        }

        void OnDrawGizmos()
        {
            if (IsDrawSphereVolume)
            {
                if (m_SphereCollider)
                {
                    Gizmos.color = SphereVolumeColor;
                    Gizmos.DrawWireSphere(this.transform.position, m_SphereCollider.radius);
                }
            }
        }
    }
}