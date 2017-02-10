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
    [SerializeField]
    private GameObject Target;

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

    private bool m_IsCanHearTarget;
    public bool IsCanHearTarget
    {
        get { return m_IsCanHearTarget; }
    }

    private List<AudioSourceDetectable> m_AudioSourcesDetected; // Running list of audio sources currently detected.

    void Awake()
    {
        m_AudioSourcesDetected = new List<AudioSourceDetectable>();
        if (Target == null)
        {
            Debug.LogError("Target is null and must be set.");
        }
    }

    // NEEDS WORK
    public void NotifyAudioDetected(AudioSourceDetectable audioSourceDetected)
    {
        // Make sure we are not already pursuing this audio source.
        if (m_AudioSourcesDetected.Contains(audioSourceDetected))
        {
            return;
        }
        // Test to see if the loudness of newly detected is louder than current.
        m_AudioSourcesDetected.Add(audioSourceDetected);
        Target = audioSourceDetected.gameObject;
        m_IsCanHearTarget = true;
    }

    // NEEDS WORK
    public void NotifyAudioLost(AudioSourceDetectable audioSourceLost)
    {
        m_AudioSourcesDetected.Remove(audioSourceLost);
        m_IsCanHearTarget = false;
    }

    // NEEDS WORK
    private void IdentifyPriorityTarget()
    {

    }

    void OnDrawGizmos()
    {
        // Draw line to target that changes if target is heard.
        if (IsDrawRaycastToTarget)
        {
            if (m_IsCanHearTarget)
            {
                Debug.DrawRay(Ears.position, Target.transform.position - Ears.position, RaycastAudibleColor);
            }
            else
            {
                Debug.DrawRay(Ears.position, Target.transform.position - Ears.position, RaycastNotAudibleColor);
            }
        }
    }
}
}
}