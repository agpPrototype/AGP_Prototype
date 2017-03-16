using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bond;
using vfx;

namespace GameCritical
{
    /// <summary>
    /// The GameController class is designed to be the only singleton in the game.
    /// </summary>
    public class GameController : MonoBehaviour 
    {
        private static GameController s_GameController = null;
        private BondManager m_BondManager;
        private SmellSmokeDriver m_SmellSmokeDriver;
        private GameObject m_Player;
        private GameObject m_Wolf;

        private ActionZone m_CurrentActionZone;

        private void Awake () 
        {
            if(s_GameController == null)
            {
                Initialize();
            }
            else
            {
                if(s_GameController != this)
                {
                    Destroy(this.gameObject);
                }
            }
        }

        // Update is called once per frame
        void Update () {

        }

        private void Initialize()
        {
            s_GameController = this;

            //initialize anything else we need here and start the game
            InitializeGame();
        }

        private void InitializeGame()
        {
            m_SmellSmokeDriver = GetComponent<SmellSmokeDriver>();
        }

        public void RegisterPlayer(GameObject player)
        {
            m_Player = player;
        }

        public void RegisterWolf(GameObject wolf)
        {
            m_Wolf = wolf;
        }

        public static GameController Instance { get { return s_GameController; } }
        public BondManager BondManager { get { return m_BondManager; } }
        public SmellSmokeDriver SmellSmokeDriver { get { return m_SmellSmokeDriver; } }
        public GameObject Player { get { return m_Player; } }
        public GameObject Wolf { get { return m_Player; } }
    }

}