using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 
/// Condition is used as part of a decision. When a parent Node wants to check if it can move to the Decision Node that
///     contains this Condition, it will check if this Condition is met. If it is, the progress of the decision tree may
///     move to the Decision that contains this Condition.
///     
/// </summary>
/// 
[System.Serializable]
public class Condition<T> : AGPMonoBehavior where T : IComparable{

    public enum ConditionComparison
    {
        Greater,
        GreaterOrEqual,
        Less,
        LessOrEqual,
        Equal,
        NotEqual
    }

    [SerializeField]
    private ConditionComparison m_Comparator;

    [SerializeField]
    private T m_Left;

    [SerializeField]
    private T m_Right;

    public Action action;

	// Use this for initialization
	void Start () {
        action = new Action();

        GetTypeDelegate getValueFct = new GetTypeDelegate(action.GetMyValue);
	}
	


	// Update is called once per frame
	void Update () {
       
	}

    public bool IsMet()
    {
        switch (m_Comparator)
        {
            case ConditionComparison.Equal:
                return (m_Left.CompareTo(m_Right) == 0);

            case ConditionComparison.NotEqual:
                return (m_Left.CompareTo(m_Right) != 0);
                
            case ConditionComparison.Greater:
                return (m_Left.CompareTo(m_Right) > 0);

            case ConditionComparison.GreaterOrEqual:
                return ( (m_Left.CompareTo(m_Right) > 0) || (m_Left.CompareTo(m_Right) == 0) );

            case ConditionComparison.Less:
                return (m_Left.CompareTo(m_Right) < 0);

            case ConditionComparison.LessOrEqual:
                return ((m_Left.CompareTo(m_Right) < 0) || (m_Left.CompareTo(m_Right) == 0));

        }

        Debug.Assert(false, "Error: Condition.cs: No comparator contained for the desired operation!");
        return false;
    }


    #region InitializeCondition Functions

    // Overload where both variables are references
    public void InitializeCondition(ref T leftHandSide, ConditionComparison comparison, ref T rightHandSide)
    {
        m_Left = leftHandSide;
        m_Right = rightHandSide;
        m_Comparator = comparison;
    }

    // Overload where neither variables are references
    public void InitializeCondition(T leftHandSide, ConditionComparison comparison, T rightHandSide)
    {
        m_Left = leftHandSide;
        m_Right = rightHandSide;
        m_Comparator = comparison;
    }

    // Overload where left variable is a references
    public void InitializeCondition(ref T leftHandSide, ConditionComparison comparison, T rightHandSide)
    {
        m_Left = leftHandSide;
        m_Right = rightHandSide;
        m_Comparator = comparison;
    }

    // Overload where right variable is a references
    public void InitializeCondition(T leftHandSide, ConditionComparison comparison, ref T rightHandSide)
    {
        m_Left = leftHandSide;
        m_Right = rightHandSide;
        m_Comparator = comparison;
    }

    #endregion

}
