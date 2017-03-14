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
            PATROLLING,
            ATTACKING,
            CHASING,
        }
        private EnemyAIState m_State;
        private ThreatLevel m_ThreatLevel;

        private BehaviorTree m_CurrentBT;
        private BehaviorTree m_PatrolBT;
        private BehaviorTree m_ChaseBT;
        private BehaviorTree m_AttackBT;
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

        [SerializeField]
        private float m_AttackRange = 6.0f;

        private Waypoint m_CurrentWaypoint;
        private NavMeshAgent m_NavAgent;
        private AILineOfSightDetection m_AILineOfSightDetection;
        private AIAudioDetection m_AIAudioDetection;

        // target related members
        private Vector3 m_TargetLastPosition;
        private bool m_IsTargetInRange;
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
            createPatrolBT();
            createAttackBT();

            m_CurrentBT = m_PatrolBT;
        }

        #region Chase Methods
        private void switchToAttackBT()
        {
            SetMainState(EnemyAIState.ATTACKING);
        }
        private bool isReachedLastSeenLocation()
        {
            if(m_NavAgent != null)
            {
                // Check if we've reached the destination
                if (!m_NavAgent.pathPending)
                {
                    if (m_NavAgent.remainingDistance <= m_NavAgent.stoppingDistance)
                    {
                        // Done pathfinding.
                        Debug.Log("AI got to target.");
                        // set this variable to true so we can progress to next node.
                        return true;
                    }
                }
            }
            else
            {
                Debug.LogError("This AI is not a NavAgent but should be!");
            }
            return false;
        }
        private void chase()
        {
            Debug.Log("AI is chasing");
            if (!m_NavAgent.isOnNavMesh)
            {
                Debug.Log("AI nav agent not on a nav mesh.");
                return;
            }

            if (m_AIAudioDetection != null && m_AILineOfSightDetection != null)
            {
                // get target position.
                if (m_Target != null)
                {
                    m_TargetLastPosition = m_Target.transform.position;
                }
            }
            else
            {
                Debug.LogError("No AIAudioDetection or AILineOfSightDetectin on AI.");
            }

            // Keep updating target position if we can see the target.
            m_NavAgent.SetDestination(m_TargetLastPosition);
            // Check to see if we have gotten close enough to target.
            m_NavAgent.stoppingDistance = StopDistFromTarget;
        }
        #endregion

        private void createChaseBT()
        {
            /// Chase_Root_Node                         (does nothing)
            ///     Chase_Node                          (chase enemy if seen or heard)
            ///         Arrive_Node                     (go to last seen position)

            DecisionNode rootNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "Chase_Root_Node");

            m_ChaseBT = new BehaviorTree(rootNode, this, "Chase BT");

            /// Chase_Node
            DecisionNode chaseNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "Chase_Node");
            chaseNode.AddAction(new Action(chase));
            m_ChaseBT.AddDecisionNodeTo(rootNode, chaseNode);

            /// Arrive_Node
            DecisionNode arriveNode = new DecisionNode(DecisionType.RepeatUntilActionComplete, "Arrive_Node");
            arriveNode.AddCondition(new Condition(isReachedLastSeenLocation));
            arriveNode.AddAction(new Action(switchToAttackBT));
            m_ChaseBT.AddDecisionNodeTo(chaseNode, arriveNode);
        }

        #region Attack Methods
        private void attack()
        {
            Debug.Log("AI is attacking");
        }
        private bool isTargetOutOfRange()
        {
            return !m_IsTargetInRange;
        }
        private bool isTargetInRange()
        {
            return m_IsTargetInRange;
        }
        private void switchToPatrolBT()
        {
            SetMainState(EnemyAIState.PATROLLING);
        }
        #endregion

        private void createAttackBT()
        {
            /// Attack_Root_Node                    (does nothing)
            ///     Basic_Attack_Node               (perform basic attack when in range)
            ///         AttackBT->LookBT_Node       (when out of attack range go back to patrolling)

            DecisionNode rootNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "Attack_Root_Node");

            m_AttackBT = new BehaviorTree(rootNode, this, "Attack BT");

            DecisionNode basicAttackNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "Basic_Attack_Node");
            basicAttackNode.AddCondition(new Condition(isTargetInRange));
            basicAttackNode.AddAction(new Action(attack));
            m_AttackBT.AddDecisionNodeTo(rootNode, basicAttackNode);

            DecisionNode attackToLookNode = new DecisionNode(DecisionType.RepeatUntilActionComplete, "AttackBT->LookBT_Node");
            attackToLookNode.AddAction(new Action(switchToPatrolBT));
            attackToLookNode.AddCondition(new Condition(isTargetOutOfRange));
            m_AttackBT.AddDecisionNodeTo(basicAttackNode, attackToLookNode);
            m_AttackBT.AddDecisionNodeTo(rootNode, attackToLookNode);
        }

        #region Patrol Methods
        private void idle()
        {
            Debug.Log("AI is idling");
        }
        private void switchToChaseBT()
        {
            SetMainState(EnemyAIState.CHASING);
        }
        private bool isHasThreat()
        {
            return m_Target != null;
        }
        private void patrol()
        {
            // Get next waypoint from patrol area if we have one
            if (PatrolArea != null)
            {
                if (m_CurrentWaypoint != null)
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
                        if (m_CurrentWaypoint != null)
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
                    if (m_CurrentWaypoint != null)
                    {
                        m_NavAgent.SetDestination(m_CurrentWaypoint.transform.position);
                    }
                }
            }
        }
        #endregion

        private void createPatrolBT()
        {
            /// Patrol_Root_Node                    (patrol the waypoints)
            ///     PatrolBT->ChaseBT               (chase enemy if seen or heard)

            /// Patrol_Node
            DecisionNode patrolNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "Patrol_Root_Node");
            patrolNode.AddAction(new Action(patrol));

            m_PatrolBT = new BehaviorTree(patrolNode, this, "Patrol BT");

            /// PatrolBT->ChaseBT
            DecisionNode patrolToChase = new DecisionNode(DecisionType.RepeatUntilActionComplete, "PatrolBT->ChaseBT_Node");
            patrolToChase.AddCondition(new Condition(new BoolTypeDelegate(isHasThreat)));
            patrolToChase.AddAction(new Action(switchToChaseBT));
            m_PatrolBT.AddDecisionNodeTo(patrolNode, patrolToChase);
        }

        private void UpdateFactors()
        {
            // get highest threat and store as target.
            m_Target = (DetectionManager.Instance.GetHighestThreat(m_AIAudioDetection, m_AILineOfSightDetection));

            // update factors if our current target isnt null.
            if (m_Target != null)
            {
                m_IsTargetInRange = Vector3.Distance(m_Target.transform.position, this.transform.position) <= m_AttackRange;
                m_TargetLastPosition = m_Target.transform.position;
            }
            else
            {
                m_IsTargetInRange = false;
            }
        }

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
            m_UpdateIntervalTimer -= Time.deltaTime;
            if (m_UpdateIntervalTimer <= 0.0f)
            {
                UpdateFactors();

                // reset the timer
                m_UpdateIntervalTimer = m_UpdateInterval;
            }
        }

        private void SetMainState(EnemyAIState newState)
        {
            m_State = newState;

            // Switch the current behavior tree to the new state's tree
            switch (m_State)
            {
                case EnemyAIState.PATROLLING:
                    m_CurrentBT = m_PatrolBT;
                    break;

                case EnemyAIState.ATTACKING:
                    m_CurrentBT = m_AttackBT;
                    break;

                case EnemyAIState.CHASING:
                    m_CurrentBT = m_ChaseBT;
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
    }
}