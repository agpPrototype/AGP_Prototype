using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace AI
{
    public delegate void VoidTypeDelegate();
    public delegate void VoidTypeDelegate1(WolfMainState State);
    public delegate bool BoolTypeDelegate();

    /// <summary>
    /// 
    /// Action class is part of a Decision Node. When the Condition of the Decision Node is met, the progression of the behavior tree moves this
    ///     Decision Node and this Action is performed. Action should contain a function pointer to some function that will contain instructions
    ///     for the AI to perform the task desired. The functions probably won't have a return type as it should point to the AI's member functions
    ///     and the logic should reside there.
    ///     
    /// </summary>
    /// 
    public class Action
    {

        // Delegate names will take the form of "<returnType> Func <#arguments>" 
        private VoidTypeDelegate m_VoidFunc0;

        private VoidTypeDelegate1 m_VoidFunc1;
        private WolfMainState m_StateToChangeTo;

        private bool m_ActionComplete;

        public Action(VoidTypeDelegate voidFunc0)
        {
            m_VoidFunc0 = voidFunc0;

            m_ActionComplete = false;

            m_VoidFunc1 = null;
        }

        public Action(VoidTypeDelegate1 voidFunc1, WolfMainState newState)
        {
            m_VoidFunc1 = voidFunc1;
            m_StateToChangeTo = newState;

            m_ActionComplete = false;

            m_VoidFunc0 = null;
        }

        public void PerformAction()
        {
            if (ReferenceEquals(m_VoidFunc1, null))
                m_VoidFunc0.Invoke();
            else
                m_VoidFunc1.Invoke(m_StateToChangeTo);
        }

        // For now has the same functionality as PerformAction but may be different in the future
        public void Continue()
        {
            if (ReferenceEquals(m_VoidFunc1, null))
                m_VoidFunc0.Invoke();
            else
                m_VoidFunc1.Invoke(m_StateToChangeTo);
        }

        public bool IsComplete()
        {
            return m_ActionComplete;
        }

        public void SetComplete(bool isComplete)
        {
            m_ActionComplete = isComplete;
        }


        // TODO:
        //
        // Will probably need the Action class to posess functionality to see if the Action is still in progress, if it is complete,
        // how long the duration of the action may take, and maybe a continue function to resume progress of the Delegate
    }

}