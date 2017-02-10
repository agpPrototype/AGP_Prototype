using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using AI;
using AI.Detection;

namespace AI
{
[RequireComponent(typeof(AILineOfSightDetection))]
[RequireComponent(typeof(AIAudioDetection))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAISM : AIStateMachine
{
    // Define possible states for AI. Subject to change.
    public enum State
    {
        CHASE,
        LOOKING,
        PATROLLING
    }

    [SerializeField]
    [Tooltip("Speed AI turns to find target.")]
    private float LookRotationSpeed = 10.0f;

    [SerializeField]
    [Tooltip("When chasing target how far away AI will stop when chasing.")]
    private float StopDistFromTarget = 5.0f;

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

    private NavMeshAgent m_NavAgent;
    private AILineOfSightDetection m_AILineOfSightDetection;
    private AIAudioDetection m_AIAudioDetection;
    private BehaviorTree m_ChaseBT;

    // Use this for initialization
    void Start()
    {
        m_State = State.LOOKING;
        m_AILineOfSightDetection = GetComponent<AILineOfSightDetection>();
        m_AIAudioDetection = GetComponent<AIAudioDetection>();
        m_NavAgent = GetComponent<NavMeshAgent>();
        InitializeStateBehaviorTrees();
    }

    // Update is called once per frame
    public override void Update()
    { 
        // Calling base update ends up calling UpdateStateMachine() in parent's parent.
        base.Update();
        UpdateStateMachine();
    }

    private void InitializeStateBehaviorTrees()
    {
        CreateChaseBT();
    }

    private void UpdateFactors()
    {

    }

    private void CreateChaseBT()
    {

    }

    private void LookForTarget()
    {
        Debug.Log("Looking for target...");
    }

    private void ChaseTarget()
    {
        Debug.Log("chasing target...");
    }

    public override void UpdateStateMachine()
    {
        if (state == State.LOOKING)
        {
            // Look for hostiles.
            if (m_AILineOfSightDetection != null)
            {
                if (m_AILineOfSightDetection.IsCanSeeTarget)
                {
                    state = State.CHASE;
                }
                else
                {
                    // rotate to try and find the target.
                    this.transform.Rotate(0, LookRotationSpeed * Time.deltaTime, 0);
                }
            }
        }
        else if (state == State.CHASE)
        {
            if (!m_NavAgent.isOnNavMesh)
            {
                Debug.Log("AI nav agent not on a nav mesh.");
                return;
            }
            // Keep updating target position if we can see the target.
            m_NavAgent.SetDestination(m_AILineOfSightDetection.TargetLastSeenPosition);
            // Check to see if we have gotten close enough to target.
            m_NavAgent.stoppingDistance = StopDistFromTarget;
            // Check if we've reached the destination
            if (!m_NavAgent.pathPending)
            {
                if (m_NavAgent.remainingDistance <= m_NavAgent.stoppingDistance)
                {
                    // Done pathfinding.
                    state = State.LOOKING;
                }
            }
        }
        else if(state == State.PATROLLING)
        {

        }
    }
}
}