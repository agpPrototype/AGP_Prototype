using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class AIStateMachine : StateMachineBase {


    public delegate void TriggerActionComplete(bool isComplete);

    public event TriggerActionComplete OnActionComplete;

    // Use this for initialization
    void Start ()
    {
	}
	
	// Update is called once per frame
	public override void Update ()
    {
        base.Update();
	}

    public void CompleteCurrentActionExternal(bool isComplete)
    {
        OnActionComplete(isComplete);
    }
}
