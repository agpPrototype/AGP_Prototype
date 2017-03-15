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
            LOOKING,
        }
        private EnemyAIState m_State;
        private ThreatLevel m_ThreatLevel;

        private BehaviorTree m_CurrentBT;
        private BehaviorTree m_PatrolBT;
        private BehaviorTree m_ChaseBT;
        private BehaviorTree m_LookBT;
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
            createLookBT();

            m_CurrentBT = m_PatrolBT;
        }

        #region Look Methods
        private bool isLookTimeOver()
        {
            // set start time and end time for timer if it hasnt been set.
            if (!m_IsLookTimerSet)
            {
                m_LookStartTime = Time.time;
                m_LookEndTime = Time.time + m_LookTime;
                m_IsLookTimerSet = true;
            }

            // check to see if timer done.
            if (Time.time < m_LookEndTime)
            {
                return false;
            }

            // if we get this far timer is done.
            m_IsLookTimerSet = false;
            return true;
        }
        #endregion

        private void createLookBT()
        {
            /// Look_Root_Node
            ///     Wait_Node
            ///         LookBT->PatrolBT_Node
            ///         LookBT->ChaseBT_Node
            DecisionNode rootNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "Look_Root_Node");
            rootNode.AddAction(new Action(idle));

            m_LookBT = new BehaviorTree(rootNode, this, "Look BT");

            /// Wait_Node
            DecisionNode lookNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "Look_Node");
            lookNode.AddAction(new Action(idle));
            m_LookBT.AddDecisionNodeTo(rootNode, lookNode);

            /// LookBT->PatrolBT
            DecisionNode lookToPatrol = new DecisionNode(DecisionType.RepeatUntilActionComplete, "LookBT->PatrolBT_Node");
            lookToPatrol.AddAction(new Action(switchToAttackBT));
            lookToPatrol.AddCondition(new Condition(isLookTimeOver));
            m_LookBT.AddDecisionNodeTo(lookNode, lookToPatrol);

            /// LookBT->ChaseBT
            DecisionNode looktoChase = new DecisionNode(DecisionType.RepeatUntilActionComplete, "LookBT->ChaseBT_Node");
            looktoChase.AddAction(new Action(switchToChaseBT));
            looktoChase.AddCondition(new Condition(isHasThreat));
            m_LookBT.AddDecisionNodeTo(lookNode, looktoChase);
        }

        #region Chase Methods
        private void switchToAttackBT()
        {
            SetMainState(EnemyAIState.ATTACKING);
        }
        private void switchToLookBT()
        {
            SetMainState(EnemyAIState.LOOKING);
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
            ///         ChaseBT->AttackBT_Node          (switch to attack BT)
            ///         ChaseBT->LookBT_Node            (switch to look BT)

            DecisionNode rootNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "Chase_Root_Node");
            rootNode.AddAction(new Action(idle));

            m_ChaseBT = new BehaviorTree(rootNode, this, "Chase BT");

            /// Chase_Node
            DecisionNode chaseNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "Chase_Node");
            chaseNode.AddAction(new Action(chase));
            m_ChaseBT.AddDecisionNodeTo(rootNode, chaseNode);

            /// ChaseBT->AttackBT
            DecisionNode chaseToAttack = new DecisionNode(DecisionType.RepeatUntilActionComplete, "ChaseBT->AttackBT_Node");
            chaseToAttack.AddAction(new Action(switchToAttackBT));
            chaseToAttack.AddCondition(new Condition(isReachedLastSeenLocation));
            chaseToAttack.AddCondition(new Condition(isTargetInRange));
            m_ChaseBT.AddDecisionNodeTo(chaseNode, chaseToAttack);

            /// ChaseBT->LookBT
            DecisionNode chaseToLook = new DecisionNode(DecisionType.RepeatUntilActionComplete, "ChaseBT->LookBT_Node");
            chaseToLook.AddAction(new Action(switchToLookBT));
            chaseToLook.AddCondition(new Condition(isReachedLastSeenLocation));
            chaseToLook.AddCondition(new Condition(isTargetOutOfRange));
            chaseToLook.AddCondition(new Condition(isNoThreat));
            m_ChaseBT.AddDecisionNodeTo(chaseNode, chaseToLook);
        }

        #region Attack Methods
        private void idle()
        {
            // do nothing just used as placeholder for callback for Nodes.
        }
        private void attack()
        {
            // ADD AS U WISH HERE SAMMY BOY! ;)
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
            ///     AttackBT->LookBT_Node       (when out of attack range go back to patrolling)

            DecisionNode rootNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "Attack_Root_Node");
            rootNode.AddAction(new Action(idle));

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
        private void switchToChaseBT()
        {
            SetMainState(EnemyAIState.CHASING);
        }
        private bool isHasThreat()
        {
            return m_Target != null;
        }
        private bool isNoThreat()
        {
            return m_Target == null;
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

                case EnemyAIState.LOOKING:
                    m_CurrentBT = m_LookBT;
                    break;

                default:
                    Debug.Log("Error: EnemyAISM.cs : No Behavior Tree to switch to for desired new state.");
                    break;


            }

            m_CurrentBT.RestartTree();
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