using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioContainer : MonoBehaviour {

    [SerializeField]
    private List<AudioClip> m_AudioClips;

    public void PlaySound(int clipIndex)
    {
        if(m_AudioClips != null)
        {
            if(clipIndex < m_AudioClips.Count)
            {
                AudioClip audioClip = m_AudioClips[clipIndex];
                if (audioClip != null)
                {
                    AudioManager.PlaySFX_3D(audioClip, 1.0f, Vector3.zero, this.transform);
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
