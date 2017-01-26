using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorTree : MonoBehaviour {

    [SerializeField]
    private CompanionAISM.WolfMainState m_TreeState;

    DecisionNode m_RootDN;

    DecisionNode m_CurrentDN;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public BehaviorTree(CompanionAISM.WolfMainState StateOfTree, DecisionNode RootNode)
    {
        m_TreeState = StateOfTree;

        m_RootDN = RootNode;
        m_CurrentDN = RootNode; // Should start tree at root node
    }

    public void AddDecisionNodeTo(DecisionNode parent, DecisionNode newNode)
    {
        parent.AddDecisionNodeLink(newNode);
    }



    public void ContinueBehaviorTree()
    {
        if (m_CurrentDN.DecisionComplete())
        {
            DecisionNode nextNode = m_CurrentDN.FindNextNode();
            
            if(ReferenceEquals(nextNode, null))
            {
                //Debug.Log("Warning: BehaviorTree.cs : Current Decision Complete but cannot move to children");
                m_CurrentDN.ProcessDecision();
            }
            else
            {
                m_CurrentDN = nextNode;
                m_CurrentDN.ProcessDecision();
            }
        }
        else
        {
            m_CurrentDN.ProcessDecision();
        }
    }

}
