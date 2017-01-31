using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 
/// DecisionNode is the main component of  our behavior tree. These will map out the progress across the tree and will essentially determine 
///     how the AI works. Each node contains one or more Conditions (that must be met in order for the parent Node to progress to this Node), 
///     an Action that tells the AI object specifically what to do, as well as possible links to determine where the AI will progress to after 
///     the contained Action is complete.
///     
///     IMPORTANT!!!!!
///**** The order that the Links are added MATTERS. The order determines a preference to which
///     Link should be tested first!
///     
/// </summary>
/// 
namespace AI
{

    /// <summary>
    /// 
    /// We will have different DecisionNode types because there can be different functionality for the Nodes
    /// 
    ///    **RepeatUntilActionComplete: This type will remain here and repeatedly perform the Action until it determines that the Action 
    ///         specified is complete. For example, if Accalia is following the player and the player stops moving, she should not stop 
    ///         running right away but should run in a curve to circle back around towards the player and end up looking at the player.
    ///         This Decision will have an Action to follow a circular path and should continue to follow the path until it is complete.
    ///         Then it may progress to a Link to change her state to IDLE.
    ///         
    ///    **RepeatUntilCanProgress: This type will repeat an Action until the Conditions have been met for one of the linked
    ///         DecisionNodes. For example, if a DecisionNode's Action is to follow the player, it will have a link to a Node that
    ///         will have a Condition that tests if the player has stopped moving. Accalia will continue to follow the player until
    ///         that Condition is met or her state is changed externally.
    ///         
    /// </summary>
    public enum DecisionType
    {
        RepeatUntilActionComplete,
        RepeatUntilCanProgress,
        SwitchStates
    }

    public class DecisionNode : AGPMonoBehavior
    {

        [SerializeField]
        private string m_nameTag;

        private Action m_Action;

        private List<Condition> m_Conditions;

        private List<DecisionNode> m_Links;

        private DecisionType m_MyType;

        private bool m_DecisionComplete;


        public DecisionNode(DecisionType decisionType, string tag)
        {
            m_MyType = decisionType;

            if (decisionType == DecisionType.RepeatUntilActionComplete)
            {
                m_DecisionComplete = false;
            }
            else
            {
                // We can just continue the Action until we can move on to another child Node
                m_DecisionComplete = true;
            }

            m_nameTag = tag;
            m_Links = new List<DecisionNode>();
            m_Conditions = new List<Condition>();
        }


        // Test that all Conditions of this node are met. If any are not, this node cannot be travelled to.
        public bool TestConditions()
        {
            // If no conditions are set, you can always move to this node (probably used when parent type is "RepeateUntilActionComplete")
            if(m_Conditions.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < m_Conditions.Count; ++i)
            {
                if (!m_Conditions[i].IsMet())
                {
                    return false;
                }
            }

            return true;
        }


        public bool IsDecisionComplete()
        {
            return m_DecisionComplete;
        }


        // This function is called when the statemachine decides to remain in the state that it is in
        public void ProcessDecision()
        {
            //Debug.Log("Current DecisionNode is " + m_nameTag.ToString());
            if (m_MyType == DecisionType.RepeatUntilActionComplete)
            {
                // test if action is complete and then see if it can move to next Node
                if (m_Action.IsComplete())
                {
                    m_DecisionComplete = true;
                    SetInternalActionComplete(false);
                    return;
                }
                else
                {
                    m_Action.Continue();
                }
            }
            else if (m_MyType == DecisionType.RepeatUntilCanProgress)
            {
                for (int i = 0; i < m_Links.Count; ++i)
                {
                    if (m_Links[i].TestConditions())
                    {
                        m_DecisionComplete = true;
                        return;
                    }
                }

                m_Action.Continue();
            }
            else if (m_MyType == DecisionType.SwitchStates)
            {
                m_Action.PerformAction();
                m_DecisionComplete = true;
            }
        }


        // For use outside of this class, should be called by the StateMachine/Behavior tree class.
        //
        // if(DecisionComplete()){
        //      CurrentNode = CurrentNode->FindNextNode()
        // else
        //      CurrentNode->ProcessDecision();
        //
        public DecisionNode FindNextNode()
        {
            for (int i = 0; i < m_Links.Count; ++i)
            {
                if (m_Links[i].TestConditions())
                {
                    m_DecisionComplete = false;
                    return m_Links[i];
                }
            }

            return null;
        }

        #region Functions to build DecisionNode
        public void AddDecisionNodeLink(DecisionNode node)
        {
            m_Links.Add(node);
        }

        public void AddCondition(Condition condition)
        {
            m_Conditions.Add(condition);
        }

        public void AddAction(Action action)
        {
            m_Action = action;
            //m_Action.SetMyNode(this);
        }

        public Action GetNodeAction()
        {
            return m_Action;
        }

        #endregion

        public void SetInternalActionComplete(bool isComplete)
        {
            if(m_MyType == DecisionType.RepeatUntilActionComplete || m_MyType == DecisionType.SwitchStates)
            {
                m_Action.SetComplete(isComplete);
            }
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
