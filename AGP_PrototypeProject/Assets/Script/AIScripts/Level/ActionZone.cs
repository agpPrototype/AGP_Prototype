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
            Debug.Log("Set Current Action Zone: " + gameObject.transform.parent.gameObject.name);
        }
        else if (other.GetComponent<AI.CompanionAISM>())
        {
            m_IsWolfInZone = true;
        }
        

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

        if (!m_IsWolfInZone && !m_IsPlayerInZone)
        {
            //GameController.Instance.CurrentActionZone = null;
            GameController.Instance.Wolf.GetComponent<AI.StealthNavigation>().CurrentActionZone = null;
            GameController.Instance.Wolf.GetComponent<AI.CompanionAISM>().SetMainState(AI.WolfMainState.Follow);
            Debug.Log("Both wolf and player left Action Zone: " + gameObject.transform.parent.gameObject.name);
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

        GameObject enemy;
        for (int i = 0; i < numChild; ++i)
        {
            enemy = m_EnemyListObjectRef.transform.GetChild(i).gameObject;
            m_EnemyList.Add(enemy);
            enemy.GetComponent<AI.EnemyAISM>().MyActionZone = this;
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
           // enemy.
        }
    }

    public GameObject GetEnemy(int idx)
    {
        if(idx < m_EnemyList.Count)
            return m_EnemyList[idx];

        return null;
    }

    public GameObject GetClosestEnemyTo(Vector3 location)
    {
        GameObject closestEnemy = null;

        float minDistSq = 1000000;

        for (int i = 0; i < m_EnemyList.Count; ++i)
        {
            float distSq = Vector3.SqrMagnitude(location - m_EnemyList[i].transform.position);
            if (distSq < minDistSq)
            {
                closestEnemy = m_EnemyList[i];
                minDistSq = distSq;
            }

        }

        return closestEnemy;
    }

    public GameObject GetClosestAgrodEnemy(Vector3 location)
    {

        GameObject closestEnemy = null;

        float minDistSq = 1000000;

        for (int i = 0; i < m_EnemyList.Count; ++i)
        {
            float distSq = Vector3.SqrMagnitude(location - m_EnemyList[i].transform.position);
            bool isAgrod = m_EnemyList[i].GetComponent<AI.EnemyAISM>().IsAgrod();
            
            if (distSq < minDistSq && isAgrod)
            {
                closestEnemy = m_EnemyList[i];
                minDistSq = distSq;
            }

        }

        return closestEnemy;
    }

    public void EnemyDestroyed(GameObject enemy)
    {
       // if (enemy)
       // {
            m_EnemyList.Remove(enemy);
       // }
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
