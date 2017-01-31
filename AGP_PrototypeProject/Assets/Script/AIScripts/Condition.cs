using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public enum ConditionComparison
    {
        Greater,
        GreaterOrEqual,
        Less,
        LessOrEqual,
        Equal,
        NotEqual
    }

    /// <summary>
    /// 
    /// Condition is used as part of a decision. When a parent Node wants to check if it can move to the Decision Node that
    ///     contains this Condition, it will check if this Condition is met. If it is, the progress of the decision tree may
    ///     move to the Decision that contains this Condition.
    ///     
    /// </summary>
    /// 
    [System.Serializable]
    public class ConditionInternal<T> : AGPMonoBehavior where T : IComparable
    {



        [SerializeField]
        private ConditionComparison m_Comparator;

        [SerializeField]
        private T m_Left;

        [SerializeField]
        private T m_Right;

        public Action action;

        // Use this for initialization
        void Start()
        {

            // GetTypeDelegate getValueFct = new GetTypeDelegate(action.GetMyValue);
        }



        // Update is called once per frame
        void Update()
        {

        }

        public void UpdateInternalData(T newData_lhs)
        {
            m_Left = newData_lhs;
        }

        public void UpdateInternalData(T newData_lhs, T newData_rhs)
        {
            m_Left = newData_lhs;
            m_Right = newData_rhs;
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
                    return ((m_Left.CompareTo(m_Right) > 0) || (m_Left.CompareTo(m_Right) == 0));

                case ConditionComparison.Less:
                    return (m_Left.CompareTo(m_Right) < 0);

                case ConditionComparison.LessOrEqual:
                    return ((m_Left.CompareTo(m_Right) < 0) || (m_Left.CompareTo(m_Right) == 0));

            }

            Debug.Assert(false, "Error: Condition.cs: Condition: No comparator contained for the desired operation!");
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

    /// <summary>
    /// 
    /// Condition is a wrapper used so that the Decision Node can contain multiple ConditionInternal<T> instances without know which type each one is.
    ///     This way, the Decision Node can have an array or list of them and check the wrapper's implementation of IsMet() which will
    ///     abstract the checking of the condition as it knows which type of condition it contains. Possibly look for a better solution?
    ///     
    /// </summary>
    /// 
    public class Condition : AGPMonoBehavior
    {
        [SerializeField]
        private VariableType m_MyType;

        ConditionInternal<Int> m_IntCond;
        ConditionInternal<Float> m_FloatCond;
        ConditionInternal<Bool> m_BoolCond;

        BoolTypeDelegate m_BoolFunc0;
        FloatTypeDelegate m_FloatFunc0_lhs;
        FloatTypeDelegate m_FloatFunc0_rhs;

        public enum VariableType
        {
            Int,
            Float,
            Bool,
            BoolFunc,  // Call a bool-returning function to test for IsMet()
            FloatDelegateAndFloat,
            TwoFloatDelegates
        }

        public bool IsMet()
        {
            switch (m_MyType)
            {
                case VariableType.Int:
                    return m_IntCond.IsMet();

                case VariableType.Float:
                    return m_FloatCond.IsMet();

                case VariableType.Bool:
                    return m_BoolCond.IsMet();

                case VariableType.BoolFunc:
                    return m_BoolFunc0.Invoke();

                case VariableType.FloatDelegateAndFloat:
                    m_FloatCond.UpdateInternalData(new Float(m_FloatFunc0_lhs.Invoke()));
                    return m_FloatCond.IsMet();

                case VariableType.TwoFloatDelegates:
                    m_FloatCond.UpdateInternalData(new Float(m_FloatFunc0_lhs.Invoke()), new Float(m_FloatFunc0_rhs.Invoke()));
                    return m_FloatCond.IsMet();
            }

            Debug.Assert(false, "Error: Condition.cs: ConditionWrapper:  No valid type of Condition<T> exists for the wrapper");
            return false;
        }


        #region Overloaded Constructors
        public Condition(Int lhs, ConditionComparison comparisonType, Int rhs)
        {
            m_MyType = VariableType.Int;
            m_IntCond = new ConditionInternal<Int>();
            m_IntCond.InitializeCondition(lhs, comparisonType, rhs);
        }

        public Condition(Float lhs, ConditionComparison comparisonType, Float rhs)
        {
            m_MyType = VariableType.Float;
            m_FloatCond = new ConditionInternal<Float>();
            m_FloatCond.InitializeCondition(lhs, comparisonType, rhs);
        }

        public Condition(FloatTypeDelegate lhsDelegate, ConditionComparison comparisonType, Float rhs)
        {
            m_MyType = VariableType.FloatDelegateAndFloat;
            m_FloatCond = new ConditionInternal<Float>();
            m_FloatFunc0_lhs = lhsDelegate;
            m_FloatCond.InitializeCondition(new Float(lhsDelegate.Invoke()), comparisonType, rhs);
        }

        public Condition(FloatTypeDelegate lhsDelegate, ConditionComparison comparisonType, FloatTypeDelegate rhsDelegate)
        {
            m_MyType = VariableType.FloatDelegateAndFloat;
            m_FloatCond = new ConditionInternal<Float>();
            m_FloatFunc0_lhs = lhsDelegate;
            m_FloatFunc0_rhs = rhsDelegate;
            m_FloatCond.InitializeCondition(new Float(lhsDelegate.Invoke()), comparisonType, new Float(rhsDelegate.Invoke()));
        }

        public Condition(Bool lhs, ConditionComparison comparisonType, Bool rhs)
        {
            m_MyType = VariableType.Bool;
            m_BoolCond = new ConditionInternal<Bool>();
            m_BoolCond.InitializeCondition(lhs, comparisonType, rhs);
        }

        public Condition(BoolTypeDelegate boolDelegate)
        {
            m_MyType = VariableType.BoolFunc;
            m_BoolFunc0 = boolDelegate;
        }
        #endregion
    }
}
