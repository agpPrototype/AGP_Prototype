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

        // For looking around state
        [SerializeField]
        [Tooltip("Amount of time the AI should look for player they lost until they continue patrolling.")]
        private float m_LookTime;
        private float m_LookEndTime;
        private float m_LookStartTime;
        private bool m_IsLookTimerSet;

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

        private void LookAroundUntilLookTimerDone()
        {
            if(!m_IsLookTimerSet)
            {
                m_LookStartTime = Time.time;
                m_LookEndTime = Time.time + m_LookTime;
                m_IsLookTimerSet = true;
            }

            if(Time.time < m_LookEndTime)
            {
                this.transform.Rotate(0, LookRotationSpeed * Time.deltaTime, 0);
                return;
            }

            m_IsLookTimerSet = false;

            // Switch states from looking around to something else.
            state = State.PATROLLING;
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
                        state = State.LOOKING_AROUND;
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

                        // Move to the waypoint
                        if (m_CurrentWaypoint != null)
                        {
                            m_NavAgent.SetDestination(m_CurrentWaypoint.transform.position);
                        }

                        // If we made it to the waypoint.
                        if (m_NavAgent.remainingDistance <= Mathf.Epsilon)
                        {
                            m_CurrentWaypoint = PatrolArea.GetRandomWaypoint();
                        }
                    }
                    else
                    {
                        m_CurrentWaypoint = PatrolArea.GetRandomWaypoint();
                        if(m_CurrentWaypoint != null)
                        {
                            m_NavAgent.SetDestination(m_CurrentWaypoint.transform.position);
                        }
                    }
                }
            }
            else if (state == State.LOOKING_AROUND)
            {
                LookAroundUntilLookTimerDone();
            }
            else if (state == State.ATTACKING)
            {
                Debug.Log("In attacking state.");
            }
        }
    }
}