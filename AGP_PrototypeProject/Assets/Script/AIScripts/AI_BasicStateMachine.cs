using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_BasicStateMachine : AIStateMachine
{

    // Define possible states for AI. Subject to change.
    public enum State
    {
        CHASE,
        IDLE
    }

    private State m_State;
    public State state
    {
        get
        {
            return m_State;
        }
        set
        {
            m_State = value;
        }
    }

    // Use this for initialization
    void Start ()
    {
        m_State = State.IDLE;
    }
	
	// Update is called once per frame
	public override void Update()
    {
        base.Update();
    }

    public override void UpdateStateMachine()
    {
        if(state == State.IDLE)
        {
            // Look for hostiles.

        }
    }
}
