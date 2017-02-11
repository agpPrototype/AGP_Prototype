using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Audio;

namespace AI
{
    namespace Detection
    {
        public class AIAudioDetection : MonoBehaviour
        {
            [Tooltip("transform that will be used for AI ear source.")]
            [SerializeField]
            private Transform Ears;

            [Tooltip("Color of raycast toward target when target not audible to AI.")]
            [SerializeField]
            private Color RaycastNotAudibleColor;

            [Tooltip("Color of raycast toward target when audible to AI.")]
            [SerializeField]
            private Color RaycastAudibleColor;

            [Tooltip("Should we draw raycast to target.")]
            [SerializeField]
            private bool IsDrawRaycastToTarget;

            private bool m_IsCanHearSomething;
            public bool IsCanHearSomething
            {
                get { return m_IsCanHearSomething; }
            }

            private List<AudioSourceDetectable> m_AudioSourcesDetected; // Running list of audio sources currently detected.

            void Awake()
            {
                m_AudioSourcesDetected = new List<AudioSourceDetectable>();
            }

            // will tell the AI that it has detected audio.
            private void NotifyAudioDetected(AudioSourceDetectable audioSourceDetected)
            {
                // Make sure we are not already pursuing this audio source.
                if (m_AudioSourcesDetected.Contains(audioSourceDetected))
                {
                    return;
                }
                // Test to see if the loudness of newly detected is louder than current.
                m_AudioSourcesDetected.Add(audioSourceDetected);
                m_IsCanHearSomething = true;
            }

            // Will tell the AI it can no longer here a suspect sound.
            private void NotifyAudioLost(AudioSourceDetectable audioSourceLost)
            {
                m_AudioSourcesDetected.Remove(audioSourceLost);
                m_IsCanHearSomething = false;
            }

            /* returns the highest priority audio that has been detected.
             * in order to actually get a sound from this function it is necessary
             * that this m_IsCanHearSomething = true. Else the AI has not heard 
             * anything.
             */
            public AudioSourceDetectable GetHighestPriorityAudioDetected()
            {
                return m_AudioSourcesDetected[m_AudioSourcesDetected.Count - 1];
            }

            void OnTriggerEnter(Collider col)
            {
                // Check to see if we heard something.
                AudioSourceDetectable audioDetectable = col.GetComponent<AudioSourceDetectable>();
                if (audioDetectable != null)
                {
                    NotifyAudioDetected(audioDetectable);
                }
            }

            void OnTriggerExit(Collider col)
            {
                // Check to see if AI lost hearing of something.
                AudioSourceDetectable audioDetectable = col.GetComponent<AudioSourceDetectable>();
                if (audioDetectable != null)
                {
                    NotifyAudioLost(audioDetectable);
                }
            }

            void OnDrawGizmos()
            {
                // Draw line to target that changes if target is heard.
                if (IsDrawRaycastToTarget)
                {
                    if (m_IsCanHearSomething)
                    {
                        AudioSourceDetectable audioDetectable = GetHighestPriorityAudioDetected();
                        Debug.DrawRay(Ears.position, audioDetectable.transform.position - Ears.position, RaycastAudibleColor);
                    }
                }
            }
        }
    }
}