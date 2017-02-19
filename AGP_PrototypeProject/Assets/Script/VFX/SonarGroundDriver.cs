using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace vfx
{
    public class SonarGroundDriver : MonoBehaviour {

        [SerializeField]
        private GameObject m_player;

        [SerializeField]
        private Material m_material;

        [SerializeField]
        private float m_maxSonarScale;

        [SerializeField]
        private float m_scaleGrowth;

        private float m_sonarScale;



        // Use this for initialization
        void Awake () 
        {
            m_sonarScale = 0;
        }

        void Update()
        {
//            if(m_sonarScale > m_maxSonarScale)
//            {
//                m_sonarScale = 0;
//            }
//            m_sonarScale += m_scaleGrowth;
            if(Input.GetKeyDown(KeyCode.G))
            {
                StartCoroutine(PlaySonarPulse());
            }
        }

        // Update is called once per frame
        void LateUpdate () 
        {
            // want to send the player position to the shader
           // m_material.SetFloat("_Radius", )
            m_material.SetVector("_PlayerPos", m_player.transform.position);
            m_material.SetFloat("_SonarScale", m_sonarScale);
        }


        IEnumerator PlaySonarPulse()
        {
            float progress = 0f;
            while(progress < m_maxSonarScale)
            {
                m_sonarScale = Mathf.Lerp(0.0f, m_maxSonarScale, progress / m_maxSonarScale);
                progress += Time.deltaTime;
                yield return null;
            }
            m_sonarScale = 0;
        }
    }

}
