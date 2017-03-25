using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameCritical;
using Utility;
using UI;

namespace EndGame
{
    public class EndGameSequence : MonoBehaviour
    {
        private static EndGameSequence s_EndGameSequence;

        //END GAME
        public delegate void CrumbleAndRiseEvent();
        public event CrumbleAndRiseEvent CrumbleAndRise;

        private EndGameCamHandler m_CamHandler;
        public EndGameCamHandler CamHandler
        {
            get
            {
                return m_CamHandler;
            }
            set
            {
                m_CamHandler = value;
            }
        }

        private bool m_PreEndGameEnded;
        public bool PreEndGameEnded
        {
            get { return m_PreEndGameEnded; }
            set { m_PreEndGameEnded = value; }
        }

        private bool m_PostEndGameStart;
        public bool PostEndGameStart
        {
            get { return m_PostEndGameStart; }
            set { m_PostEndGameStart = value; }
        }

        void Awake()
        {
            if (s_EndGameSequence == null)
            {
                s_EndGameSequence = this;
            }
            else
            {
                if (s_EndGameSequence != this)
                {
                    Destroy(this.gameObject);
                }
            }
        }

        void Start()
        {
            GameController.Instance.EndGame += DoEndGame;
        }

        void DoEndGame(EnumService.GameState state)
        {
            if (state == EnumService.GameState.Win_SwitchActivated)
            {
                CrumbleAndRise();
            }
        }

        public static EndGameSequence Instance { get { return s_EndGameSequence; } }

        void OnDestroy()
        {
            GameController.Instance.EndGame -= DoEndGame;
        }
    }
}
