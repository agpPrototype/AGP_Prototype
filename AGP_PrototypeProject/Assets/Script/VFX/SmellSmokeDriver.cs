using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;
namespace vfx 
{
    public class SmellSmokeDriver : MonoBehaviour 
    {
        [SerializeField]private GameObject m_VFXCamera;
        private Camera m_mainCam;
        private GameObject[] m_SmokeSystems;

        private bool m_isSmoking;

        // Use this for initialization
        void Awake() 
        {
            m_isSmoking = false;
            m_mainCam = Camera.main;
           
            m_SmokeSystems = GameObject.FindGameObjectsWithTag("SmellSmoke");

            //disable because find only works for active objects
            for (int i = 0; i < m_SmokeSystems.Length; i++)
            {
                m_SmokeSystems[i].SetActive(false);
            }
        }

        private void EnableSmellSmoke()
        {
            m_VFXCamera.SetActive(true);
            m_mainCam.GetComponent<ColorCorrectionCurves>().enabled = true;
            m_mainCam.GetComponent<VignetteAndChromaticAberration>().enabled = true;
            for (int i = 0; i < m_SmokeSystems.Length; i++)
            {
                if(m_SmokeSystems[i])
                    m_SmokeSystems[i].SetActive(true);
            }
        }

        private void DisableSmellSmoke()
        {
            m_VFXCamera.SetActive(false);
            m_mainCam.GetComponent<ColorCorrectionCurves>().enabled = false;
            m_mainCam.GetComponent<VignetteAndChromaticAberration>().enabled = false;
            for (int i = 0; i < m_SmokeSystems.Length; i++)
            {
                if (m_SmokeSystems[i])
                    m_SmokeSystems[i].SetActive(false);
            }
        }

        public void ToggleSmellSmoke()
        {
            if(!m_isSmoking)
            {
                EnableSmellSmoke();
                StartCoroutine(CoolToggle(0.1f));
            }
            else
            {
                DisableSmellSmoke();
                StartCoroutine(CoolToggle(0.1f));
            }
        }

        private IEnumerator CoolToggle(float sec)
        {
            yield return new WaitForSeconds(sec);
            m_isSmoking = !m_isSmoking;
        }
    }
}

