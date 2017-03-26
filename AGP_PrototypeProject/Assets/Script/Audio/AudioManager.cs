using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour {

	public static AudioManager Instance = null;

	[SerializeField]
	private AudioSource audioSourcePrefab;

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			if (Instance != this)
			{
				Destroy(this.gameObject);
			}
		}
	}

	// plays a 3d sound at a position relative to the passed parent transform
	// by default the transform is null and the relativePos parameter will be in world space
	// otherwise, the relativePos parameter will be in the parent space
	public static void PlaySFX_3D(AudioClip clip, float volume, Vector3 relativePos, Transform parent = null)
	{
		if(Instance != null)
		{
			Instance.Play3DSound(clip, volume, relativePos, parent);
		}
	}

	// plays a 2d sound
	public static void PlaySFX_2D(AudioClip clip, float volume)
	{
		if(Instance != null)
		{
			Instance.Play2DSound(clip, volume);
		}
	}

	// Plays a sound that is in 3D space and can be parented to an object.
	private void Play3DSound(AudioClip clip, float volume, Vector3 relativePos, Transform parent)
	{
		AudioSource audInstance = (AudioSource)Instantiate(audioSourcePrefab, parent);

		audInstance.transform.localPosition = relativePos;

		StartCoroutine(PlaySoundCoroutine(audInstance, clip, volume, 1.0f));
	}

	// Plays sound that is in 2D space so no depth/falloff to sound.
	private void Play2DSound(AudioClip clip, float volume)
	{
		AudioSource audInstance = (AudioSource)Instantiate(audioSourcePrefab);

		StartCoroutine(PlaySoundCoroutine(audInstance, clip, volume, 0.0f));
	}

	private IEnumerator PlaySoundCoroutine(AudioSource audInstance, AudioClip clip, float volume, float spatial)
	{
		audInstance.clip = clip;
		audInstance.volume = volume;
		audInstance.spatialBlend = spatial;
		audInstance.Play();

		audInstance.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

		// Destroy audio source after it has completely played.
		yield return new WaitForSeconds(audInstance.clip.length);
		Destroy(audInstance.gameObject);
	}
}
