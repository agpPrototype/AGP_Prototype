using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// AUTHOR: michael feng
/// 
/// DESCRIPTION: This script lerps the activating and deactivating of the enemy's detection beam
/// 
/// </summary>
public class EnemyDetectionBeam : MonoBehaviour {

	[Tooltip("How forceful the beam Opens/Closes")]
	[SerializeField]
	private float m_Spring = 1.0f;
	public float Spring { get { return m_Spring; } set { m_Spring = value; } }

	[Tooltip("Spotlight")]
	[SerializeField]
	private Light Light;

	[Tooltip("GameObject that represents volumetric lighting")]
	[SerializeField]
	private GameObject Volume;

	[Tooltip("Audio Source for beam SFX")]
	[SerializeField]
	private AudioSource Audio;

	private bool m_IsOpen = false;
	public bool IsOpen
	{
		get
		{
			return m_IsOpen;
		}
		set
		{
			m_IsOpen = value;
			m_ShouldLerpLight = true;
			m_ShouldLerpVolume = true;
			m_ShouldLerpAudio = true;
		}
	}

	private bool m_ShouldLerpLight;
	private bool m_ShouldLerpVolume;
	private bool m_ShouldLerpAudio;
	private float m_LightMaxAngle;
	private float m_VolumeMaxScale;
	private float m_AudioMaxVolume;

	void Start()
	{
		m_ShouldLerpLight = true;
		m_ShouldLerpVolume = true;
		m_ShouldLerpAudio = true;
		if(Light != null)
		{
			m_LightMaxAngle = Light.spotAngle;
		}
		else
		{
			Debug.Log("Enemy Spotlight not referenced.");
		}

		if(Volume != null)
		{
			m_VolumeMaxScale = Volume.transform.localScale.x;
		}
		else
		{
			Debug.Log("Enemy Volumetric object not referenced.");
		}

		if (Audio != null)
		{
			m_AudioMaxVolume = Audio.volume;
			Audio.volume = 0.0f;
		}
		else
		{
			Debug.Log("Enemy Audio Source not referenced.");
		}
	}

	void Update()
	{

		if (Light != null && m_ShouldLerpLight)
		{
			float eps = float.Epsilon;
			Light.enabled = true;
			float desiredLightAngle = m_IsOpen ? m_LightMaxAngle : 1.0f;
			float currentLightAngle = Light.spotAngle;

			float velLight = (desiredLightAngle - currentLightAngle) * m_Spring;

			float toAddLight = velLight * Time.deltaTime;
				
			if (Mathf.Abs(toAddLight) > eps)
			{
				Light.spotAngle += toAddLight;
			}
			else
			{
				Light.spotAngle = desiredLightAngle;
				m_ShouldLerpLight = false;

				if (Light.spotAngle == 1.0f)
				{
					Light.enabled = false;
				}
			}

		}

		if (Volume != null && m_ShouldLerpVolume)
		{
			float eps = float.Epsilon;
			Volume.SetActive(true);
			float desiredVolumeScale = m_IsOpen ? m_VolumeMaxScale : 0.0f;
			float currentVolumeScale = Volume.transform.localScale.x;

			float velVolume = (desiredVolumeScale - currentVolumeScale) * m_Spring;

			float toAddVolume = velVolume * Time.deltaTime;

			if (Mathf.Abs(toAddVolume) > eps)
			{
				Volume.transform.localScale = new Vector3(Volume.transform.localScale.x + toAddVolume, Volume.transform.localScale.y, Volume.transform.localScale.z + toAddVolume);
			}
			else
			{
				Volume.transform.localScale = new Vector3(desiredVolumeScale, Volume.transform.localScale.y, desiredVolumeScale);
				m_ShouldLerpVolume = false;
				if (Volume.transform.localScale.x == 0.0f)
				{
					Volume.SetActive(false);
				}
			}
		}

		if(Audio != null && m_ShouldLerpAudio)
		{
			float eps = float.Epsilon;
			Audio.enabled = true;
			float desiredAudioVolume = m_IsOpen ? m_AudioMaxVolume : 0.0f;
			float currentAudioVolume = Audio.volume;

			float velAudio = (desiredAudioVolume - currentAudioVolume) * m_Spring;

			float toAddAudio = velAudio * Time.deltaTime;

			if(Mathf.Abs(toAddAudio) > eps)
			{
				Audio.volume = Audio.volume + toAddAudio;
			}
			else
			{
				Audio.volume = desiredAudioVolume;
				m_ShouldLerpAudio = false;
				if(Audio.volume == 0.0f)
				{
					Audio.enabled = false;
				}
			}
		}
	}
}
