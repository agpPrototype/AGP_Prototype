using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base State
/// </summary>
public class State {


}


/// <summary>
/// Base Class for all state machines
/// </summary>
abstract public class StateMachineBase : AGPMonoBehavior {

        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        public virtual void Update() {
            UpdateStateMachine();
        }


        abstract public void UpdateStateMachine();

        //public void SetState()
    
}
