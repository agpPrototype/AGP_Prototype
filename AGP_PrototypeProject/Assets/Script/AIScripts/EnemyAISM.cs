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
            CHASING,
            PATROLLING,
            ATTACKING,
            LOOKING_AROUND
        }

        [SerializeField]
        [Tooltip("Speed AI turns to find target.")]
        private float LookRotationSpeed = 10.0f;

        [SerializeField]
        [Tooltip("When chasing target how far away AI will stop when chasing.")]
        private float StopDistFromTarget = 5.0f;

        [SerializeField]
        [Tooltip("Patrol area for AI to patrol in or around.")]
        private PatrolArea PatrolArea;

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

        private Waypoint m_CurrentWaypoint;
        private NavMeshAgent m_NavAgent;
        private AILineOfSightDetection m_AILineOfSightDetection;
        private AIAudioDetection m_AIAudioDetection;

        // Use this for initialization
        void Start()
        {
            m_State = State.PATROLLING;
            m_AILineOfSightDetection = GetComponent<AILineOfSightDetection>();
            m_AIAudioDetection = GetComponent<AIAudioDetection>();
            m_NavAgent = GetComponent<NavMeshAgent>();
        }

        // Update is called once per frame
        public override void Update()
        { 
            // Calling base update ends up calling UpdateStateMachine() in parent's parent.
            base.Update();
            UpdateStateMachine();
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
            // Use AI senses to sense things suspicious.
            {
                // Look for things.
                if (m_AILineOfSightDetection != null)
                {
                    if (m_AILineOfSightDetection.IsCanSeeTarget)
                    {
                        state = State.CHASING;
                    }
                }
                // Listen for things.
                if(m_AIAudioDetection != null)
                {
                    if(m_AIAudioDetection.IsCanHearSomething)
                    {
                        Debug.Log("AI hears something suspicious!");
                    }
                }
                // Choose state based on senses.
            }

            if (state == State.CHASING)
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
                        state = State.ATTACKING;
                    }
                }
            }
            else if(state == State.PATROLLING)
            {
                // Get next waypoint from patrol area if we have one
                if(PatrolArea != null)
                {
                    if(m_CurrentWaypoint != null)
                    {
                        if (!m_NavAgent.isOnNavMesh)
                        {
                            Debug.Log("AI nav agent not on a nav mesh.");
                            return;
                        }

                        // If we made it to the waypoint.
                        if(m_NavAgent.remainingDistance <= Mathf.Epsilon)
                        {
                            m_CurrentWaypoint = PatrolArea.GetRandomWaypoint(); //PatrolArea.GetNextWaypoint(m_CurrentWaypoint);
                            if (m_CurrentWaypoint != null)
                            {
                                m_NavAgent.SetDestination(m_CurrentWaypoint.transform.position);
                            }
                        }
                    }
                    else
                    {
                        m_CurrentWaypoint = PatrolArea.GetNextWaypoint(null);
                        if(m_CurrentWaypoint != null)
                        {
                            m_NavAgent.SetDestination(m_CurrentWaypoint.transform.position);
                        }
                    }
                }
            }
            else if (state == State.LOOKING_AROUND)
            {
                // rotate to try and find the target.
                this.transform.Rotate(0, LookRotationSpeed * Time.deltaTime, 0);
            }
            else if (state == State.ATTACKING)
            {
                Debug.Log("In attacking state.");
            }
        }
    }
}