using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bond;
using vfx;
using Utility;
using EndGame;

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

        private GameObject m_Player;
        private GameObject m_Wolf;
        private EnumService.GameState m_GameState;
        public EnumService.GameState GameState
        {
            get { return m_GameState; }
            set
            {
                m_GameState = value;
                if (m_GameState == EnumService.GameState.Win_BellRing)
                {
                    //fire endgame event
                    EndGame(m_GameState);
                }
            }
        }

        //END GAME
        public delegate void EndGameEvent(EnumService.GameState state);
        public event EndGameEvent EndGame;


        [SerializeField]
        private GameObject m_AllActionZonesRef;
        private ActionZone[] m_ActionZoneList;

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

            m_GameState = EnumService.GameState.InGame;

            //initialize peacetrees
            GameObject peaceTrees = GameObject.Find("PeaceTrees");
            if (peaceTrees)
            {
                peaceTrees.SetActive(true);
            }
        }

        public void RegisterPlayer(GameObject player)
        {
            m_Player = player;
        }

        public void RegisterWolf(GameObject wolf)
        {
            m_Wolf = wolf;
        }

        public void RegisterEnemy(GameObject enemy)
        {
            GetActionZoneFromPoint(enemy.transform.position).RegisterEnemy(enemy);
        }

        public ActionZone GetActionZoneFromPoint(Vector3 location)
        {
            for(int i = 0; i < m_ActionZoneList.Length; ++i)
            {
                if (m_ActionZoneList[i].IsLocationInZone(location))
                {
                    return m_ActionZoneList[i];
                }
            }

            return null;
        }

        //public GameObject FindClosestAgrodEnemy()
        //{

        //}

        public static GameController Instance { get { return s_GameController; } }
        public BondManager BondManager { get { return m_BondManager; } }
        public SmellSmokeDriver SmellSmokeDriver { get { return m_SmellSmokeDriver; } }
        public GameObject Player { get { return m_Player; } }
        public GameObject Wolf { get { return m_Wolf; } }

        public ActionZone CurrentActionZone {
            get { return m_CurrentActionZone; }
            set{ m_CurrentActionZone = value; }
        }
    }

}