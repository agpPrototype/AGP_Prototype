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
/// </summary>
/// 
public class DecisionNode : AGPMonoBehavior {

    private Action m_Action;

    private List<Condition> m_Conditions;

    private List<DecisionNode> m_Links;

    private DecisionType m_MyType;

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
        RepeatUntilCanProgress
    }

    // Test that all Conditions of this node are met. If any are not, this node cannot be travelled to.
    public bool TestConditions()
    {
        for(int i = 0; i < m_Conditions.Count; ++i)
        {
            if (!m_Conditions[i].IsMet())
            {
                return false;
            }
        }

        return true;
    }

    // This function is called when the statemachine decides to remain in the state that it is in
    public void ProcessDecision()
    {
        if(m_MyType == DecisionType.RepeatUntilActionComplete)
        {
            // test if action is complete and then
            if (TestConditions())
            {
                DecisionNode next = FindNextNode();

                if(next == null)
                {
                    Debug.Log("Warning: DecisionNode.cs : no action to perform but cannot progress to next node");
                }
                else
                {
                    // Progress to next Node
                }
            }
        }
    }

    private DecisionNode FindNextNode()
    {
        for (int i = 0; i < m_Links.Count; ++i)
        {
            if (m_Links[i].TestConditions())
            {
                return m_Links[i];
            }
        }

        return null;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
