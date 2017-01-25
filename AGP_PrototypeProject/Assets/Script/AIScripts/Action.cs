using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public delegate void VoidTypeDelegate();
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
public class Action  {

    // Delegate names will take the form of "<returnType> Func <#arguments>" 
    VoidTypeDelegate m_VoidFunc0;


    public Action(VoidTypeDelegate voidFunc)
    {
        m_VoidFunc0 = voidFunc;
    }

    public void PerformAction()
    {
        m_VoidFunc0.Invoke();
    }

    // For now has the same functionality as PerformAction but may be different in the future
    public void Continue()
    {
        m_VoidFunc0.Invoke();
    }


    // TODO:
    //
    // Will probably need the Action class to posess functionality to see if the Action is still in progress, if it is complete,
    // how long the duration of the action may take, and maybe a continue function to resume progress of the Delegate
}
