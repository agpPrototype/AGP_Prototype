using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioContainer : MonoBehaviour {

    [SerializeField]
    private List<AudioClip> m_AudioClips;

    [SerializeField]
    private List<float> m_Volumes;

    void Start()
    {
        if(m_AudioClips == null)
        {
            m_AudioClips = new List<AudioClip>();
        }
        if(m_Volumes == null)
        {
            m_Volumes = new List<float>();
        }

        int sizeDiff = m_AudioClips.Count - m_Volumes.Count;
        for (int i = 0; i < sizeDiff; i++)
        {
            m_Volumes.Add(1.0f);
        }
    }

    public void PlaySound(int clipIndex)
    {
        if(clipIndex < 0)
        {
            return;
        }

        if(m_AudioClips != null)
        {
            if(clipIndex < m_AudioClips.Count)
            {
                AudioClip audioClip = m_AudioClips[clipIndex];
                if (audioClip != null)
                {
                    float volume = 1.0f;
                    if (clipIndex < m_Volumes.Count)
                    {
                        volume = m_Volumes[clipIndex];
                    }
                    AudioManager.PlaySFX_3D(audioClip, volume, Vector3.zero, this.transform);
                }
                else
                {
                    Debug.LogError("audio clip is null.");
                }
            }
            else
            {
                Debug.LogError("Clip index " + clipIndex + " is out of audio clip list bounds.");
            }
        }
        else
        {
            Debug.LogError("No audio clips in audio container.");
        }
    }

}

public enum AccaliaSounds
{
    HOWL,
    BARK1, BARK2,
    GROWL1, GROWL2, GROWL3,
    WHINE1, WHINE2,
    COMPLAIN, WHINE3,
    DAMAGED

}
