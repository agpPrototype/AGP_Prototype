using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EndGame
{
    public class PeaceTreeHandler : MonoBehaviour
    {
        private Animator m_Animator;

        void Awake()
        {
            
        }

        void OnDestroy()
        {
            EndGameSequence.Instance.CrumbleAndRise -= DoEndGame;
        }

        // Use this for initialization
        void Start()
        {
            EndGameSequence.Instance.CrumbleAndRise += DoEndGame;
            m_Animator = GetComponent<Animator>();
            gameObject.SetActive(false);
        }

        public void DoEndGame()
        {
            StartCoroutine(DoRise());
            if (m_Animator)
            {
                gameObject.SetActive(true);
                m_Animator.SetTrigger("Rise");
            }
        }

        IEnumerator DoRise()
        { 
            while (!EndGameSequence.Instance.PreEndGameEnded)
            {
                yield return null;
            }

            if (m_Animator)
            {
                m_Animator.SetTrigger("Rise");
            }
        }

    }
}
