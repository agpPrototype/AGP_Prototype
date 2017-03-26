using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// author: Rob Neir
/// </summary>
/// 

namespace UI
{
    [RequireComponent(typeof(AudioSource))]
    public class Menu : UIComponent
    {
        public AudioClip m_HoverSound;
        public AudioClip m_ClickSound;

        [SerializeField]
        [Tooltip("Button first selected when menu opens.")]
        protected Button m_StartingSelectedButton;

        private AudioSource m_AudioSource;

        void Start()
        {
            m_AudioSource = GetComponent<AudioSource>();
            if(m_StartingSelectedButton != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(m_StartingSelectedButton.gameObject);
            }
        }

        public virtual void Show() { }

        /* callback function for button click audio. */
        public void PlayClickSound()
        {
            if (m_ClickSound != null)
            {
                if (m_AudioSource != null)
                {
                    m_AudioSource.clip = m_ClickSound;
                    m_AudioSource.Play();
                }
            }
        }

        /* callback function for button hover audio. */
        public void PlayHoverSound()
        {
            if (m_AudioSource && m_AudioSource.clip && m_HoverSound != null)
            {
                if(m_AudioSource != null)
                {
                    m_AudioSource.clip = m_HoverSound;
                    m_AudioSource.Play();
                }
            }
        }
    } // Menu class
} // UI namespace
