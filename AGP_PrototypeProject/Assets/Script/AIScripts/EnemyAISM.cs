using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using AI.Detection;
using AI;

namespace AI
{
    [RequireComponent(typeof(AILineOfSightDetection))]
    [RequireComponent(typeof(AIAudioDetection))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAISM : AIStateMachine
    {
        #region AI State Members
        public enum EnemyAIState
        {
            IDLE,
            CHASING,
            PATROLLING,
            ATTACKING,
            LOOKING_AROUND
        }
        private EnemyAIState m_State;
        private ThreatLevel m_ThreatLevel;

        private BehaviorTree m_CurrentBT;
        private BehaviorTree m_IdleBT;
        private BehaviorTree m_ChaseBT;
        private BehaviorTree m_PatrolBT;
        #endregion

        [SerializeField]
        [Tooltip("Speed AI turns to find target.")]
        private float LookRotationSpeed = 10.0f;

        [SerializeField]
        [Tooltip("When chasing target how far away AI will stop when chasing.")]
        private float StopDistFromTarget = 5.0f;

        [SerializeField]
        [Tooltip("When moving between waypoints the distance to stop when reaching a point.")]
        private float StopDistFromWaypoints = 0.3f;

        [SerializeField]
        [Tooltip("Patrol area for AI to patrol in or around.")]
        private PatrolArea PatrolArea;

        [SerializeField]
        [Tooltip("Intervals between each update for AI intelligence: Line of sight, Audio detection, etc.")]
        private float m_UpdateInterval = 0.6f;
        private float m_UpdateIntervalTimer = 0.0f;

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

        private AIDetectable m_Target; // current prioritized target.

        // Use this for initialization
        private void Start()
        {
            m_State = EnemyAIState.PATROLLING;
            m_UpdateIntervalTimer = m_UpdateInterval;
            m_AILineOfSightDetection = GetComponent<AILineOfSightDetection>();
            m_AIAudioDetection = GetComponent<AIAudioDetection>();
            m_NavAgent = GetComponent<NavMeshAgent>();
            initBehaviorTrees();
        }

        private void initBehaviorTrees()
        {
            createChaseBT();

            m_CurrentBT = m_IdleBT;
        }

        private void createChaseBT()
        {
            /// Node
            /* node to perform anything AI does while idling */
            DecisionNode rootNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "Root_Idle_Node");
            rootNode.AddAction(new Action(idle));

            m_IdleBT = new BehaviorTree(rootNode, this, "Idle State BT");

            /// Node
            /* node to switch from idle to chasing state */
            DecisionNode chaseNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "Chase_Node");
            chaseNode.AddAction(new Action(chase));
            /* add condition to chase node for when AI detects an enemy */
            Condition threatCond = new Condition(new BoolTypeDelegate(isHasThreat));
            chaseNode.AddCondition(threatCond);
            /* add the chase node to the tree */
            m_IdleBT.AddDecisionNodeTo(rootNode, chaseNode);

            /// Node
            DecisionNode resetNode = new DecisionNode(DecisionType.RepeatUntilActionComplete, "Chase->Idle");
            resetNode.AddAction(new Action(new VoidTypeDelegate(m_IdleBT.RestartTree)));
            Condition noThreatCond = new Condition(new BoolTypeDelegate(isNotHasThreat));
            resetNode.AddCondition(noThreatCond);

            m_IdleBT.AddDecisionNodeTo(chaseNode, resetNode);
        }

        #region Chase BT Methods
        private bool isHasThreat()
        {
            return (DetectionManager.Instance.GetHighestThreat(m_AIAudioDetection, m_AILineOfSightDetection) != null);
        }
        private bool isNotHasThreat()
        {
            return (DetectionManager.Instance.GetHighestThreat(m_AIAudioDetection, m_AILineOfSightDetection) == null);
        }
        private void idle()
        {
            Debug.Log("AI is idling");
        }
        private void chase()
        {
            Debug.Log("AI is chasing");
        }
        #endregion

        // Update is called once per frame
        public override void Update()
        {
            // Calling base update ends up calling UpdateStateMachine() in parent's parent.
            UpdateStateMachine();

            /* traverse current behavior tree */
            m_CurrentBT.ContinueBehaviorTree();
        }

        public override void UpdateStateMachine()
        {
            /*
            m_UpdateIntervalTimer -= Time.deltaTime;
            if (m_UpdateIntervalTimer <= 0.0f)
            {
                // reset the timer
                m_UpdateIntervalTimer = m_UpdateInterval;
                m_Target = DetectionManager.Instance.GetHighestThreat(m_AIAudioDetection, m_AILineOfSightDetection);
            }*/
        }

        private void SetMainState(EnemyAIState newState)
        {
            m_State = newState;

            //m_CurrentBT.RestartTree();

            // Switch the current behavior tree to the new state's tree
            switch (m_State)
            {
                case EnemyAIState.IDLE:
                    m_CurrentBT = m_IdleBT;
                    break;

                case EnemyAIState.CHASING:
                    m_CurrentBT = m_ChaseBT;
                    break;

                case EnemyAIState.PATROLLING:
                    m_CurrentBT = m_PatrolBT;
                    break;

                default:
                    Debug.Log("Error: EnemyAISM.cs : No Behavior Tree to switch to for desired new state.");
                    break;


            }

            m_CurrentBT.RestartTree();
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
                /*if(m_TargetLastSeenVelocity != null)
                {
                    Vector3 lastSeenVelocity = m_TargetLastSeenVelocity;
                    float step = LookRotationSpeed * Time.deltaTime;
                    Vector3 newDir = Vector3.RotateTowards(transform.forward, lastSeenVelocity, step, 0.0f);
                    transform.rotation = Quaternion.LookRotation(newDir);
                    this.transform.Rotate(0, LookRotationSpeed * Time.deltaTime, 0);
                    return;
                }*/
            }

            m_IsLookTimerSet = false;

            // Switch states from looking around to something else.
            m_State = EnemyAIState.PATROLLING;
        }

        private void OnDrawGizmos()
        {
            if (m_Target)
            {
                Debug.DrawLine(m_AILineOfSightDetection.Apex.position, m_Target.transform.position, Color.red);
            }
        }

#if false
            if (state == State.CHASING)
            {
                if (!m_NavAgent.isOnNavMesh)
                {
                    Debug.Log("AI nav agent not on a nav mesh.");
                    return;
                }
                // Keep updating target position if we can see the target.
                m_NavAgent.SetDestination(m_TargetLastSeenPosition);
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
                        //m_NavAgent.Resume();// SetDestination(m_CurrentWaypoint.transform.position);
                        m_NavAgent.SetDestination(m_CurrentWaypoint.transform.position);

                        // If we made it to the waypoint.
                        if (m_NavAgent.remainingDistance <= StopDistFromWaypoints)
                        {
                            m_CurrentWaypoint = PatrolArea.GetRandomWaypoint();
                            if(m_CurrentWaypoint != null)
                            {
                                m_NavAgent.SetDestination(m_CurrentWaypoint.transform.position);
                            }
                            else
                            {
                                Debug.Log("Randomly generated AI waypoint destination is null.");
                            }
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
#endif
    }
}

/*private void updateLastSeenVariables(GameObject target)
{
    m_TargetLastSeenPosition = target.transform.position;
    Rigidbody rigidBody = target.GetComponentInParent<Rigidbody>();
    if(rigidBody != null)
    {
        m_TargetLastSeenVelocity = rigidBody.velocity;
    }
}*/
