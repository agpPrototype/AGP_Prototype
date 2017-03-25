using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// author: Rob Neir
/// </summary>
/// 

namespace UI
{
    [RequireComponent(typeof(AudioSource))]
    public class Menu : MonoBehaviour
    {
        public AudioClip m_HoverSound;
        public AudioClip m_ClickSound;

        private AudioSource m_AudioSource;

        void Start()
        {
            m_AudioSource = GetComponent<AudioSource>();
        }

        /* callback function for button click audio. */
        public void PlayClickSound()
        {
            if (m_ClickSound != null)
            {
                m_AudioSource.clip = m_ClickSound;
                m_AudioSource.Play();
            }
        }

        /* callback function for button hover audio. */
        public void PlayHoverSound()
        {
            if (m_AudioSource && m_AudioSource.clip && m_HoverSound != null)
            {
                m_AudioSource.clip = m_HoverSound;
                m_AudioSource.Play();
            }
        }
    } // Menu class
} // UI namespace
