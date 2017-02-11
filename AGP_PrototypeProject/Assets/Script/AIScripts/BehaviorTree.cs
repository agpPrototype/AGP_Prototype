using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public class BehaviorTree : MonoBehaviour
    {

        [SerializeField]
        private WolfMainState m_TreeState;

        private string m_StateTag;

        DecisionNode m_RootDN;
        DecisionNode m_CurrentDN;

        AIStateMachine m_OwningSM;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public BehaviorTree(DecisionNode RootNode, AIStateMachine OwningSM, string NameTag)
        {
            m_StateTag = NameTag;

            m_RootDN = RootNode;
            m_CurrentDN = RootNode; // Should start tree at root node

            m_OwningSM = OwningSM;
        }

        /// <summary>
        /// Use this constructor for wolf state trees. If enemy AI use the other constructor for now. StateOfTree has no impact on logic, 
        /// simply there for debugging purposes to see what tree you are currently in.
        /// </summary>
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
            //Debug.Log("Current BT is " + m_TreeState.ToString());
            if (m_CurrentDN.IsDecisionComplete())
            {
                DecisionNode nextNode = m_CurrentDN.FindNextNode();

                if (ReferenceEquals(nextNode, null))
                {
                    //Debug.Log("Warning: BehaviorTree.cs : Current Decision Complete but cannot move to children");
                    m_OwningSM.OnActionComplete -= new AIStateMachine.TriggerActionComplete(m_CurrentDN.SetInternalActionComplete);
                    m_CurrentDN.ProcessDecision();
                }
                else
                {
                    m_CurrentDN.SetInternalActionComplete(false);
                    m_CurrentDN = nextNode;
                    m_CurrentDN.ProcessDecision();
                    m_OwningSM.OnActionComplete -= new AIStateMachine.TriggerActionComplete(m_CurrentDN.SetInternalActionComplete);
                    m_OwningSM.OnActionComplete += new AIStateMachine.TriggerActionComplete(m_CurrentDN.SetInternalActionComplete);
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
            m_OwningSM.OnActionComplete -= new AIStateMachine.TriggerActionComplete(m_CurrentDN.SetInternalActionComplete);
            m_OwningSM.OnActionComplete += new AIStateMachine.TriggerActionComplete(m_CurrentDN.SetInternalActionComplete);
        }

    }
}