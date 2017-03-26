using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManagerTester : MonoBehaviour {

	[SerializeField]
	private AudioClip clip;

	[SerializeField]
	private Transform parent;

	[SerializeField]
	private float timeToRepeat = 0.5f;

	private float m_Timer = 0.0f;
	
	// Update is called once per frame
	void Update () {
		if(m_Timer >= timeToRepeat)
		{
			m_Timer = 0.0f;
			AudioManager.PlaySFX_3D(clip, 1.0f, new Vector3(0,0,0) , parent);
		}
		else
		{
			m_Timer += Time.deltaTime;
		}
	}
}
