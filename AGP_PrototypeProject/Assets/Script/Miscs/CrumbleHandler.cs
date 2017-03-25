using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace EndGame
{
    public class CrumbleHandler : MonoBehaviour
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

        }

        public void DoEndGame()
        {
            StartCoroutine(DoCrumble());
            
        }

        IEnumerator DoCrumble()
        {
            while (!EndGameSequence.Instance.PreEndGameEnded)
            {
                yield return null;
            }

            if (m_Animator)
            {
                m_Animator.SetTrigger("Crumble");
            }
        }
    }
}
