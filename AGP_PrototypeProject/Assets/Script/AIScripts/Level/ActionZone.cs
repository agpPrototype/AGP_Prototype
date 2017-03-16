using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameCritical;

public class ActionZone : MonoBehaviour {

    private Collider m_ZoneCollider;

    [SerializeField]
    private GameObject m_StealthPosGraph;

    [SerializeField]
    private GameObject m_FinalStealthPos;
    public GameObject FinalStealthPos {  get { return m_FinalStealthPos; } }

    [SerializeField]
    private GameObject m_EnemyListObjectRef;

    private GameObject[] m_StealthPointList;
    public GameObject[] StealthPointList
    {
        get { return m_StealthPointList; }
    }

    private List<GameObject> m_EnemyList;

    private bool m_IsWolfInZone = false;
    private bool m_IsPlayerInZone = false;


    // Use this for initialization
    void Start () {

        m_ZoneCollider = GetComponent<Collider>();
        Debug.Assert(m_ZoneCollider != null, "No Collider for zone");

        RegisterEnemiesInZone();

        InitializeStealthGraph();
	}

    private void InitializeStealthGraph()
    {
        // Get all child game objects
        int numChild = m_StealthPosGraph.transform.childCount;
        m_StealthPointList = new GameObject[numChild];

        for (int i = 0; i < numChild; ++i)
        {
            m_StealthPointList[i] = m_StealthPosGraph.transform.GetChild(i).gameObject;
        }

        Debug.Assert(m_StealthPointList.Length > 1, "ERROR: Need stealth points on map in order to have AI Navigate!");
    }

    void OnTriggerEnter(Collider other)
    {
        // Tell wolf that player entered the zone
        if (other.GetComponent<Player.PlayerControl>())
        {
            m_IsPlayerInZone = true;
            GameController.Instance.Wolf.GetComponent<AI.CompanionAISM>().PlayerLeftActionZone(false);

            GameController.Instance.CurrentActionZone = this;
            GameController.Instance.Wolf.GetComponent<AI.StealthNavigation>().CurrentActionZone = this;
            Debug.Log("Set Current Action Zone");
        }
        else if (other.GetComponent<AI.CompanionAISM>())
        {
            m_IsWolfInZone = true;
        }
        // If Enemy, register it with this Zone
        

    }

    void OnTriggerExit(Collider other)
    {
        // Tell Wolf that Player left the zone
        if (other.GetComponent<Player.PlayerControl>())
        {
            m_IsPlayerInZone = false;
            GameController.Instance.Wolf.GetComponent<AI.CompanionAISM>().PlayerLeftActionZone(true);
        }
        else if (other.GetComponent<AI.CompanionAISM>())
        {
            m_IsWolfInZone = false;

        }
        //else if (other.GetComponent<AI.EnemyAISM>())
        //{
        //    m_EnemyList.Remove(other.gameObject);
        //}

        if (!m_IsWolfInZone && !m_IsPlayerInZone)
        {
            GameController.Instance.CurrentActionZone = null;
            GameController.Instance.Wolf.GetComponent<AI.StealthNavigation>().CurrentActionZone = null;
            GameController.Instance.Wolf.GetComponent<AI.CompanionAISM>().SetMainState(AI.WolfMainState.Follow);
            Debug.Log("Both wolf and player left Action Zone");
        }
    }

    public bool IsLocationInZone(Vector3 location)
    {
        return GetComponent<Collider>().bounds.Contains(location);
    }

    private void RegisterEnemiesInZone()
    {
        // Get all child game objects
        int numChild = m_EnemyListObjectRef.transform.childCount;
        m_EnemyList = new List<GameObject>();

        for (int i = 0; i < numChild; ++i)
        {
            m_EnemyList.Add(m_EnemyListObjectRef.transform.GetChild(i).gameObject);
        }

        Debug.Assert(m_EnemyList.Count > 0, "Warning: No enemies set in an action zone. Continue if this is expected behavior.");
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void RegisterEnemy(GameObject enemy)
    {
        if (enemy)
        {
            m_EnemyList.Add(enemy);
        }
    }

    public void EnemyDestroyed(GameObject enemy)
    {
        if (enemy)
        {
            m_EnemyList.Remove(enemy);
        }
    }

    public int GetNumEnemiesAlive()
    {
        return m_EnemyList.Count;
    }

    public bool IsAtFinalStealthPoint(Vector3 curLocation)
    {
        // If all enemies are dead, no need to stealth
        if (m_EnemyList.Count == 0)
        {
            return true;
        }
        else
        {
            float distToFinalSq = (m_FinalStealthPos.transform.position - curLocation).sqrMagnitude;
            float distThreshSq = 2.5f;

            if(distToFinalSq < distThreshSq)
            {
                return true;
            }

            return false;
        }
    }

}
