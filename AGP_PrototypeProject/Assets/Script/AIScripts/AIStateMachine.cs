using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AIDetection))]
abstract public class AIStateMachine : StateMachineBase {

	// Use this for initialization
	void Start ()
    {
	}
	
	// Update is called once per frame
	public override void Update ()
    {
        base.Update();
	}
}
