using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AILineOfSightDetection))]
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

    private AILineOfSightDetection m_AILineOfSightDetection;

    // Use this for initialization
    void Start ()
    {
        m_State = State.IDLE;
        m_AILineOfSightDetection = GetComponent<AILineOfSightDetection>();
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
            if(m_AILineOfSightDetection != null)
            {
                if(m_AILineOfSightDetection.IsCanSeePlayer)
                {
                    state = State.CHASE;
                }
            }
        }
        else if(state == State.CHASE)
        {

        }
    }
}
