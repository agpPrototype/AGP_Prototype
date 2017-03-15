using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;
namespace vfx 
{
    public class SmellSmokeDriver : MonoBehaviour 
    {
        [SerializeField]private GameObject m_VFXCamera;
        private Camera m_MainCam;
        private GameObject[] m_SmokeSystems;

        private bool m_IsSmoking;
        private bool m_CanActivateSmell;

        // Use this for initialization
        void Awake() 
        {
            m_IsSmoking = false;
            m_CanActivateSmell = true; //cooldown
            m_MainCam = Camera.main;
           
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
            m_MainCam.GetComponent<ColorCorrectionCurves>().enabled = true;
            m_MainCam.GetComponent<VignetteAndChromaticAberration>().enabled = true;
            for (int i = 0; i < m_SmokeSystems.Length; i++)
            {
                m_SmokeSystems[i].SetActive(true);
            }
        }

        private void DisableSmellSmoke()
        {
            m_VFXCamera.SetActive(false);
            m_MainCam.GetComponent<ColorCorrectionCurves>().enabled = false;
            m_MainCam.GetComponent<VignetteAndChromaticAberration>().enabled = false;
            for (int i = 0; i < m_SmokeSystems.Length; i++)
            {
                m_SmokeSystems[i].SetActive(false);
            }
        }

        public void ToggleSmellSmoke()
        {
            if(m_CanActivateSmell)
            {
                m_CanActivateSmell = false;
                if(!m_IsSmoking)
                {
                    EnableSmellSmoke();
                    StartCoroutine(CoolToggle(0.3f));
                }
                else
                {
                    DisableSmellSmoke();
                    StartCoroutine(CoolToggle(0.3f));
                }
            }

        }

        private IEnumerator CoolToggle(float sec)
        {
            yield return new WaitForSeconds(sec);
            m_IsSmoking = !m_IsSmoking;
            m_CanActivateSmell = true;
        }
    }
}

