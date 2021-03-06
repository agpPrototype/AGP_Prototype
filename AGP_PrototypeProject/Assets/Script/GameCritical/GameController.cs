﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bond;
using vfx;
using Utility;
using EndGame;
using Player;
using AI;
using UI;
using UnityEngine.SceneManagement;

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
        private EndGameSequence m_EndGameSequence;
        private PlayerControl m_PlayerControl;
        private HUDCanvas m_HUDCanvas;

        private GameObject m_Player;
        private GameObject m_Wolf;
        private CompanionAISM m_cAISM;
        private EnumService.GameState m_GameState;
        public EnumService.GameState GameState
        {
            get { return m_GameState; }
            set
            {
                m_GameState = value;
                HandleEventChanged();              
            }
        }

        #region Events
        //end game
        public delegate void EndGameEvent(EnumService.GameState state);
        public event EndGameEvent EndGame;

        //interrupt game
        public delegate void GameInterruptedEvent(EnumService.GameState state);
        public event GameInterruptedEvent GameInterruption;
        #endregion

        [SerializeField]
        private GameObject m_AllActionZonesRef;
        private ActionZone[] m_ActionZoneList;

        private ActionZone m_CurrentActionZone;

        #region Cheat
        [Header("-Cheat-")]
        public bool CheatWin;

        #endregion


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

            // Extract all Action Zones from game object reference
            // Get all child game objects
            int numChild = m_AllActionZonesRef.transform.childCount;
            m_ActionZoneList = new ActionZone[numChild];

            for (int i = 0; i < numChild; ++i)
            {
                m_ActionZoneList[i] = m_AllActionZonesRef.transform.GetChild(i).GetChild(0).gameObject.GetComponent<ActionZone>();
            }

            Debug.Assert(m_ActionZoneList.Length > 0, "ERROR: Need ActionZones on map!");
        }

        // Update is called once per frame
        void Update () {
            #region Cheat
            if (CheatWin && m_GameState != EnumService.GameState.Win_SwitchActivated)
            {
                m_GameState = EnumService.GameState.Win_SwitchActivated;
                EndGame(EnumService.GameState.Win_SwitchActivated);
            }
            #endregion
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

            m_BondManager = GetComponent<BondManager>();

            m_EndGameSequence = GetComponent<EndGameSequence>();

            m_HUDCanvas = GetComponentInChildren<HUDCanvas>();

            m_GameState = EnumService.GameState.InGame;

            //initialize peacetrees
            GameObject peaceTrees = GameObject.Find("PeaceTrees");
            if (peaceTrees)
            {
                peaceTrees.SetActive(true);
            }
        }

        public void RegisterPlayer(PlayerControl player)
        {
            m_Player = player.gameObject;
            m_PlayerControl = player;
        }

        public void RegisterWolf(CompanionAISM compAISM)
        {
            m_Wolf = compAISM.gameObject;
            m_cAISM = compAISM;
            //m_PlayerControl.GetComponent<CommandHandler>().SetCompanionAISM(compAISM);
        }

        public void RegisterEnemy(GameObject enemy)
        {
            GetActionZoneFromPoint(enemy.transform.position).RegisterEnemy(enemy);
        }

        public ActionZone GetActionZoneFromPoint(Vector3 location)
        {
            if (!m_CurrentActionZone || m_ActionZoneList == null)
                return null;

            for(int i = 0; i < m_ActionZoneList.Length; ++i)
            {
                if (m_ActionZoneList[i].IsLocationInZone(location))
                {
                    return m_ActionZoneList[i];
                }
            }

            return null;
        }

        public ActionZone[] GetAllActionZones()
        {
            return m_ActionZoneList;
        }

        public List<GameObject> GetAllEnemies()
        {
            ActionZone[] actionZones = GetAllActionZones();
            List<GameObject> enemies = new List<GameObject>();

            if (actionZones != null)
            {
                for (int i = 0; i < actionZones.Length; i++)
                {
                    ActionZone az = actionZones[i];
                    if (az != null)
                    {
                        GameObject[] azEnemies = az.EnemyList;
                        if (azEnemies != null)
                        {
                            enemies.AddRange(azEnemies);
                        }
                    }
                }
            }

            return enemies;
        }

        //public GameObject FindClosestAgrodEnemy()
        //{

        //}

        public static GameController Instance { get { return s_GameController; } }
        public BondManager BondManager { get { return m_BondManager; } }
        public SmellSmokeDriver SmellSmokeDriver { get { return m_SmellSmokeDriver; } }
        public GameObject Player { get { return m_Player; } }
        public GameObject Wolf { get { return m_Wolf; } }
        public EndGameSequence EndGameSequence { get { return m_EndGameSequence; } }
        public PlayerControl PlayerControl { get { return m_PlayerControl; } }
        public HUDCanvas HUDCanvas { get { return m_HUDCanvas; } }
        public CompanionAISM CompanionAISM { get { return m_cAISM; } }

        public ActionZone CurrentActionZone {
            get { return m_CurrentActionZone; }
            set{ m_CurrentActionZone = value; }
        }

        private void HandleEventChanged()
        {
            switch (m_GameState)
            {
                case EnumService.GameState.Win_SwitchActivated:
                    EndGame(m_GameState);
                    break;
                case EnumService.GameState.InPauseMenu:
                case EnumService.GameState.InGame:
                case EnumService.GameState.InTutorial:
                    GameInterruption(m_GameState);
                    break;

            }

        }

        public void RestartGameOnDeath(string deathText)
        {
            StartCoroutine("DoRestartingOnDeath", deathText);
        }

        IEnumerator DoRestartingOnDeath(string deathText)
        {
            if (m_HUDCanvas)
            {
                m_HUDCanvas.ShowDeathText(deathText);
            }
            yield return new WaitForSeconds(3);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        }
    }

}