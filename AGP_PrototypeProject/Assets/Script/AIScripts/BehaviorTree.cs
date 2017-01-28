using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public class BehaviorTree : MonoBehaviour
    {

        [SerializeField]
        private WolfMainState m_TreeState;

        DecisionNode m_RootDN;
        DecisionNode m_CurrentDN;

        CompanionAISM m_OwningSM;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public BehaviorTree(WolfMainState StateOfTree, DecisionNode RootNode, CompanionAISM OwningSM)
        {
            m_TreeState = StateOfTree;

            m_RootDN = RootNode;
            m_CurrentDN = RootNode; // Should start tree at root node

            m_OwningSM = OwningSM;
        }

        public void AddDecisionNodeTo(DecisionNode parent, DecisionNode newNode)
        {
            parent.AddDecisionNodeLink(newNode);
        }



        public void ContinueBehaviorTree()
        {
            Debug.Log("Current BT is " + m_TreeState.ToString());
            if (m_CurrentDN.IsDecisionComplete())
            {
                DecisionNode nextNode = m_CurrentDN.FindNextNode();

                if (ReferenceEquals(nextNode, null))
                {
                    //Debug.Log("Warning: BehaviorTree.cs : Current Decision Complete but cannot move to children");
                    m_OwningSM.OnActionComplete -= new CompanionAISM.TriggerActionComplete(m_CurrentDN.GetNodeAction().SetComplete);
                    m_CurrentDN.ProcessDecision();
                }
                else
                {
                    m_CurrentDN = nextNode;
                    m_CurrentDN.ProcessDecision();
                    m_OwningSM.OnActionComplete += new CompanionAISM.TriggerActionComplete(m_CurrentDN.GetNodeAction().SetComplete);
                }
            }
            else
            {
                m_CurrentDN.ProcessDecision();
            }
        }


        public void RestartTree()
        {
            m_CurrentDN = m_RootDN;
            m_OwningSM.OnActionComplete -= new CompanionAISM.TriggerActionComplete(m_CurrentDN.GetNodeAction().SetComplete);
            m_OwningSM.OnActionComplete += new CompanionAISM.TriggerActionComplete(m_CurrentDN.GetNodeAction().SetComplete);
        }

    }
}