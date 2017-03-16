using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionZone : MonoBehaviour {

    private Collider m_ZoneCollider;

    [SerializeField]
    private GameObject m_StealthPosGraph;

    private GameObject[] m_StealthPointList;
    public GameObject[] StealthPointList
    {
        get { return m_StealthPointList; }
    }


    // Use this for initialization
    void Start () {

        m_ZoneCollider = GetComponent<BoxCollider>();
        Debug.Assert(m_ZoneCollider != null, "No Collider for zone");

        InitializeStealthGraph();
	}
	
	// Update is called once per frame
	void Update () {
		
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

}
