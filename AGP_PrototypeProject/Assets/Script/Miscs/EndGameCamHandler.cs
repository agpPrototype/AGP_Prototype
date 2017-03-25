using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CameraController;

namespace EndGame
{
    public class EndGameCamHandler : MonoBehaviour
    {

        private Animator m_Animator;
        private Camera m_gameCam;
        private Vector3 m_TargetPos;
        private Quaternion m_TargetRot;
        private float m_TurningRate = 30f;
        private float m_Distance;
        private float m_StarTime;

        void OnDestroy()
        {
            EndGameSequence.Instance.CrumbleAndRise -= DoEndGame;
        }

        // Use this for initialization
        void Start()
        {
            m_TargetPos = new Vector3(42.56f, 8.52f, -12.48f);
            m_TargetRot = Quaternion.Euler(16.033f, -40.466f, 0f);
            m_gameCam = Camera.main;
            EndGameSequence.Instance.CrumbleAndRise += DoEndGame;
            m_Animator = GetComponent<Animator>();
            GetComponent<Camera>().enabled = false;
            m_Animator.enabled = false;
        }

        public void DoEndGame()
        {
            if (m_gameCam)
            {
                transform.position = m_gameCam.transform.position;
                transform.rotation = m_gameCam.transform.rotation;

                m_gameCam.enabled = false;
            }
            GetComponent<Camera>().enabled = true;
            Vector3 oldPos = transform.position;
            Quaternion oldRot = transform.rotation;
            StartCoroutine(MoveToGameCam(oldPos, oldRot, 2f));                          
        }

        IEnumerator MoveToGameCam(Vector3 oldPos, Quaternion oldRot, float overTime)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < overTime)
            {       
                transform.position = Vector3.Lerp(oldPos, m_TargetPos, elapsedTime / overTime);
                transform.rotation = Quaternion.Lerp(oldRot, m_TargetRot, elapsedTime / overTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            m_Animator.enabled = true;
            EndGameSequence.Instance.PreEndGameEnded = true;
            m_Animator.SetTrigger("EndGame");
        }
    }
}
