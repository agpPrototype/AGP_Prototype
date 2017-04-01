using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AI.Detection;
using Player;

[RequireComponent(typeof(Slider))]
public class AudioSlider : MonoBehaviour {

    [SerializeField]
    [Tooltip("AIAudible component of player")]
    private AIAudible m_AIAudible;

    [SerializeField]
    [Tooltip("Lerp speed 1.0f = immediate 0.0f = don't do this!")]
    float m_LerpSpeed = 0.05f;

    private Slider m_Slider;

    private float m_TargetVolume;

	void Start () {
        m_Slider = GetComponent<Slider>();

        // get audible component from Player
        PlayerControl playerControl = FindObjectOfType<PlayerControl>();
        if(playerControl != null)
        {
            m_AIAudible = playerControl.GetComponent<AIAudible>();
        }
        else
        {
            Debug.LogWarning("AudioSlider could not find a player to get AIAudible component from.");
        }
	}
	
	void Update () {
        // Update slider position
        if(m_AIAudible != null)
        {
            m_TargetVolume = m_AIAudible.Range / m_AIAudible.MaxRange;
            m_Slider.value = Mathf.Lerp(m_Slider.value, m_TargetVolume, m_LerpSpeed);
            if(Mathf.Abs(m_Slider.value - m_TargetVolume) < 0.01f) // snap to target volume if close enough.
            {
                m_Slider.value = m_TargetVolume;
            }
        }
	}
}
