using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using AI.Detection;
using AI;
using HealthCare;
using Audio;

namespace AI
{
    [RequireComponent(typeof(AILineOfSightDetection))]
    [RequireComponent(typeof(AIAudioDetection))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(AudioContainer))]
    public class EnemyAISM : AIStateMachine
    {
        private enum EnemyAIAnimation
        {
            Walking,
            Idling,
            Attacking
        }
        private void setAnimation(EnemyAIAnimation anim)
        {
            if (anim == EnemyAIAnimation.Walking)
            {
				SetAudioIsWalking(true);
                m_Animator.SetBool("Walk", true);
                m_Animator.SetBool("Attack", false);
            }
            else if (anim == EnemyAIAnimation.Idling)
			{
				SetAudioIsWalking(false);
				m_Animator.SetBool("Walk", false);
                m_Animator.SetBool("Attack", false);
            }
            else if (anim == EnemyAIAnimation.Attacking)
			{
				SetAudioIsWalking(false);
				m_Animator.SetBool("Walk", false);
                int IDAttack = Random.Range(1, 4);
                m_Animator.SetInteger("IDAttack", IDAttack);
                m_Animator.SetBool("Attack", true);
                
            }
        }

        #region Member Variables
        #region AI State Members
        public enum EnemyAIState
        {
            PATROLLING,
            ATTACKING,
            CHASING,
            LOOKING,
            DEAD,
        }

        private EnemyAIState m_State;
        public EnemyAIState CurrentState { get { return m_State; } }

        private BehaviorTree m_CurrentBT;
        private BehaviorTree m_PatrolBT;
        private BehaviorTree m_ChaseBT;
        private BehaviorTree m_LookBT;
        private BehaviorTree m_AttackBT;
        private BehaviorTree m_DeadBT;
        #endregion

        #region AI Settings Members
        [SerializeField]
        [Tooltip("Speed AI turns to find target.")]
        private float m_LookRotationSpeed = 0.04f;

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
        [Tooltip("Distance when AI starts attacking.")]
        private float m_AttackRange = 6.0f;

        [SerializeField]
        [Tooltip("Amount of time the AI should look for player they lost until they continue patrolling.")]
        private float m_LookTime;

        [SerializeField]
        [Tooltip("Damage amount per attack.")]
        private float m_AttackDamage = 5.0f;

        [SerializeField]
        [Tooltip("Once is chasing something it will always keep track of it within this distance.")]
        private float m_TrackDistance = 15.0f;

        [SerializeField]
        [Tooltip("Radius which if other enemies are within it, they will be notified if this AI detects an enemy.")]
        private float m_NotifyRadius = 10.0f;
        private float m_NotifyRadiusSquared;
        #endregion

        #region Timer Members (general timer members to be used for any timer in AI)
        [SerializeField]
        [Tooltip("Timer to clear the targets that this AI ignores. This simulates a memory of sorts.")]
        private float m_IgnorableTargetsClearTimer;

        private float m_WaypointWaitTime;
        private float m_TimerEndTime;
        private float m_TimerStartTime;
        private bool m_IsTimerSet;
        #endregion

        #region Component Members
        private Waypoint m_CurrentWaypoint;
        private NavMeshAgent m_NavAgent;
        private AILineOfSightDetection m_AILineOfSightDetection;
        private AIAudioDetection m_AIAudioDetection;
        private Animator m_Animator;
        private AudioSource m_WalkingAudioSource;
        private AudioContainer m_AudioContainer;
        #endregion

        #region Target Members (used to keep track of target)
        // target related members
        private Vector3 m_TargetLastPosition;
        private Vector3 m_TargetLastVelocity;
        private bool m_IsTargetInAttackRange;
        private AIDetectable m_Target; // current prioritized target.
        public AIDetectable Target {
            get
            {
                return m_Target;
            }
            set
            {
                if (m_Target == null)
                {
                    m_Target = value;
                }
                else if(value.ThreatLevel > m_Target.ThreatLevel)
                {
                    m_Target = value;
                }
            }
        }
        #endregion

        #region Debug Members
        [SerializeField]
        private bool m_DebugDontMove = false;
        #endregion
        #endregion

        private ActionZone m_MyActionZone;
        public ActionZone MyActionZone
        {
            get { return m_MyActionZone; }
            set { m_MyActionZone = value; }
        }

        void Awake()
        {
            m_UpdateIntervalTimer = m_UpdateInterval;
            m_NotifyRadiusSquared = m_NotifyRadius * m_NotifyRadius;
        }

        private void Start()
        {
            m_AILineOfSightDetection = GetComponent<AILineOfSightDetection>();
            m_AIAudioDetection = GetComponent<AIAudioDetection>();
            m_NavAgent = GetComponent<NavMeshAgent>();
            m_Animator = GetComponent<Animator>();
            m_WalkingAudioSource = GetComponent<AudioSource>();
            m_AudioContainer = GetComponent<AudioContainer>();
            m_CurrentWaypoint = PatrolArea.GetNextWaypoint(null);
            InitBehaviorTrees();
            SetMainState(EnemyAIState.PATROLLING);
        }

        private void InitBehaviorTrees()
        {
            CreateChaseBT();
            CreatePatrolBT();
            CreateAttackBT();
            CreateLookBT();
            CreateDeadBT();

            m_CurrentBT = m_PatrolBT;
        }

        #region Switch BT functions / idle function
        private void idle()
        {
            // do nothing just used as placeholder for callback for Nodes.
        }
        private void switchToChaseBT()
        {
            SetMainState(EnemyAIState.CHASING);
		}
        private void switchToPatrolBT()
        {
            SetMainState(EnemyAIState.PATROLLING);
		}
        private void switchToAttackBT()
        {
            SetMainState(EnemyAIState.ATTACKING);
		}
        private void switchToLookBT()
        {
            setAnimation(EnemyAIAnimation.Idling);
			SetMainState(EnemyAIState.LOOKING);

			if (m_AILineOfSightDetection != null)
			{
				m_AILineOfSightDetection.Beam.IsOpen = false;
			}
			else
			{
				Debug.LogError("No AILineOfSightDetectin on AI.");
			}
		}
        #endregion

        #region Look Methods
        private bool IsLookTimeOver()
        {
            // set start time and end time for timer if it hasnt been set.
            if (!m_IsTimerSet)
            {
                m_TimerStartTime = Time.time;
                m_TimerEndTime = Time.time + m_LookTime;
                m_IsTimerSet = true;
            }

            // check to see if timer done.
            if (Time.time < m_TimerEndTime)
            {
                return false;
            }

            // if we get this far timer is done.
            m_IsTimerSet = false;
            return true;
        }
        private void RotateTowardLastSeenVelocity()
        {
            if(!m_DebugDontMove)
            {
                Vector3 targetLastVelNoY = new Vector3(m_TargetLastVelocity.x, 0, m_TargetLastVelocity.z); // take out y component of last seen velocity.
                Vector3 newDir = Vector3.RotateTowards(transform.forward, targetLastVelNoY.normalized, m_LookRotationSpeed, 0.0f);
                Debug.DrawRay(transform.position, newDir, Color.red);
                transform.rotation = Quaternion.LookRotation(newDir);
            }
        }
        #endregion

        private void CreateLookBT()
        {
            /// Look_Root_Node
            ///     Wait_Node
            ///         LookBT->PatrolBT_Node
            ///         LookBT->ChaseBT_Node
            DecisionNode rootNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "Look_Root_Node");
            rootNode.AddAction(new Action(idle));

            m_LookBT = new BehaviorTree(rootNode, this, "Look BT");

            /// Look_Node
            DecisionNode lookNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "Look_Node");
            lookNode.AddAction(new Action(RotateTowardLastSeenVelocity));
            m_LookBT.AddDecisionNodeTo(rootNode, lookNode);

            /// LookBT->PatrolBT
            DecisionNode lookToPatrol = new DecisionNode(DecisionType.RepeatUntilActionComplete, "LookBT->PatrolBT_Node");
            lookToPatrol.AddAction(new Action(switchToPatrolBT));
            lookToPatrol.AddCondition(new Condition(IsLookTimeOver));
            m_LookBT.AddDecisionNodeTo(lookNode, lookToPatrol);
        }

        #region Chase Methods
        private bool IsReachedLastSeenLocation()
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
        private void Chase()
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

                m_AILineOfSightDetection.Beam.IsOpen = true;
            }
            else
            {
                Debug.LogError("No AIAudioDetection or AILineOfSightDetectin on AI.");
            }

            if (!m_DebugDontMove)
            {
                // Keep updating target position if we can see the target.
                m_NavAgent.SetDestination(m_TargetLastPosition);
                setAnimation(EnemyAIAnimation.Walking);
            }
            // Check to see if we have gotten close enough to target.
            m_NavAgent.stoppingDistance = StopDistFromTarget;
        }
        #endregion

        private void CreateChaseBT()
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
            chaseNode.AddAction(new Action(Chase));
            m_ChaseBT.AddDecisionNodeTo(rootNode, chaseNode);

            /// ChaseBT->AttackBT
            DecisionNode chaseToAttack = new DecisionNode(DecisionType.SwitchStates, "ChaseBT->AttackBT_Node");
            chaseToAttack.AddAction(new Action(switchToAttackBT));
            chaseToAttack.AddCondition(new Condition(IsTargetInAttackRange));
            chaseToAttack.AddCondition(new Condition(IsTargetHaveHealthComp));
            m_ChaseBT.AddDecisionNodeTo(chaseNode, chaseToAttack);

            /// ChaseBT->LookBT
            DecisionNode chaseToLook = new DecisionNode(DecisionType.SwitchStates, "ChaseBT->LookBT_Node");
            chaseToLook.AddAction(new Action(switchToLookBT));
            chaseToLook.AddCondition(new Condition(IsReachedLastSeenLocation));
            chaseToLook.AddCondition(new Condition(IsTargetNotHaveHealthComp));
            m_ChaseBT.AddDecisionNodeTo(chaseNode, chaseToLook);
        }

        #region Attack Methods
        public void ApplyDamageToTarget()
        {
            // get health and apply damage
            if (m_Target != null)
            {
                Health health = GetComponent<Health>();
                m_AttackDamage = health.Damage;
                PlayerHealth playerHealth = m_Target.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(m_AttackDamage, gameObject);
                }
                AccaliaHealth accaliaHealth = m_Target.GetComponent<AccaliaHealth>();
                if (accaliaHealth != null)
                {
                    accaliaHealth.TakeDamage(m_AttackDamage, gameObject);
                }
            }
        }
        private void NotifyEnemiesInRange()
        {
            List<GameObject> enemies = GameCritical.GameController.Instance.GetAllEnemies();
            for(int i = 0; i < enemies.Count; i++)
            {
                GameObject go = enemies[i];
                if(go != null)
                {
                    float dist = (go.transform.position - this.transform.position).sqrMagnitude;
                    if(dist <= m_NotifyRadiusSquared)
                    {
                        EnemyAISM enemy = go.GetComponent<EnemyAISM>();
                        if(enemy != null)
                        {
                            enemy.Target = m_Target;
                        }
                    }
                }
            }
        }
        private void Attack()
        {
            if(!m_DebugDontMove)
            {
                // set animator to attack for AI
                setAnimation(EnemyAIAnimation.Attacking);
                // Notify all other robots within radius
                NotifyEnemiesInRange();
                // rotate toward the target we are attacking at all times.
                Vector3 targetDir = m_Target.transform.position - this.transform.position;
                targetDir.y = 0; // no y component of target direction so AI wont rotate upward or down.
                Vector3 newDir = Vector3.RotateTowards(this.transform.forward, targetDir, Mathf.PI * 2, 0.0f);
                this.transform.rotation = Quaternion.LookRotation(newDir, this.transform.up);
            }
        }
        private bool IsTargetOutOfAttackRangeOrDead()
        {
            // check to see if our target is dead. there are two types, accalia and player.
            if (m_Target != null)
            {
                PlayerHealth playerHealth = m_Target.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    if(playerHealth.IsDead)
                    {
                        return true;
                    }
                }
                AccaliaHealth accaliaHealth = m_Target.GetComponent<AccaliaHealth>();
                if (accaliaHealth != null)
                {
                    if (accaliaHealth.IsDead)
                    {
                        return true;
                    }
                }
            }
            else
            {
                return true; // because target is null get out of this function.
            }

            // check to see if within attack range
            return !m_IsTargetInAttackRange;
        }
        private bool IsTargetInAttackRange()
        {
            return m_IsTargetInAttackRange;
        }
        private bool IsTargetHaveHealthComp()
        {
            if(m_Target)
            {
                if (m_Target.GetComponent<PlayerHealth>() != null)
                    return true;
                else if (m_Target.GetComponent<AccaliaHealth>() != null)
                    return true;
            }
            return false;
        }
        private bool IsTargetNotHaveHealthComp()
        {
            if (m_Target)
            {
                if (m_Target.GetComponent<PlayerHealth>() != null ||
                    m_Target.GetComponent<AccaliaHealth>() != null)
                    return false;
            }
            return true;
        }
        #endregion

        private void CreateAttackBT()
        {
            /// Root_Attack_Node                    (performs basic attack)
            ///     AttackBT->LookBT_Node           (will switch to look bt when target not in attack range)

            DecisionNode basicAttackNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "Root_Attack_Node");
            basicAttackNode.AddAction(new Action(Attack));

            m_AttackBT = new BehaviorTree(basicAttackNode, this, "Attack BT");

            DecisionNode attackToLookNode = new DecisionNode(DecisionType.SwitchStates, "AttackBT->LookBT_Node");
            attackToLookNode.AddAction(new Action(switchToLookBT));
            attackToLookNode.AddCondition(new Condition(IsTargetOutOfAttackRangeOrDead));
            m_AttackBT.AddDecisionNodeTo(basicAttackNode, attackToLookNode);
        }

        #region Patrol Methods
        private bool WaitAtWaypoint()
        {
            // set start time and end time for timer if it hasnt been set.
            if (!m_IsTimerSet)
            {
                m_TimerStartTime = Time.time;
                m_TimerEndTime = Time.time + m_CurrentWaypoint.WaitTime;
                m_IsTimerSet = true;
            }

            // check to see if timer done.
            if (Time.time < m_TimerEndTime)
            {
                return false;
            }

            // if we get this far timer is done.
            m_IsTimerSet = false;
            m_CurrentWaypoint = PatrolArea.GetNextWaypoint(m_CurrentWaypoint); 
            // notify decision node that action complete.
            return true;
        }
        private bool ArrivedAtWaypoint()
        {
            if (m_CurrentWaypoint != null)
            {
                // If we made it to the waypoint.
                if (m_NavAgent.remainingDistance < m_NavAgent.stoppingDistance)
                {
                    setAnimation(EnemyAIAnimation.Idling);
                    return true;
                }
            }
            else
            {
                Debug.Log("Current waypoint is null so AI cant get next waypoint.");
            }
            return false;
        }
        private void PatrolToNextWaypoint()
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

                    if (!m_DebugDontMove)
                    {
                        // Move to the waypoint
                        m_NavAgent.SetDestination(m_CurrentWaypoint.transform.position);
                        m_NavAgent.stoppingDistance = StopDistFromWaypoints;
                        setAnimation(EnemyAIAnimation.Walking);
                    }
                }
                else
                {
                    Debug.Log("No waypoint target specified for EnemyAISM.");
                }
            }
            else
            {
                Debug.Log("No patrol area for EnemyAISM.");
            }
        }
        #endregion

        private void CreatePatrolBT()
        {
            /// Patrol_Root_Node                (patrol the waypoints until get to new waypoint)
            ///     Wait_Waypoint_Node          (wait at waypoint until time complete)
            ///         Patrol_Root_Node        (when AI is done waiting at waypoint they will return to patrol node)
            ///     

            /// Patrol_Node
            DecisionNode patrolNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "Patrol_Root_Node");
            patrolNode.AddAction(new Action(PatrolToNextWaypoint));
            patrolNode.AddCondition(new Condition(WaitAtWaypoint));

            m_PatrolBT = new BehaviorTree(patrolNode, this, "Patrol BT");

            /// Wait_Waypoint_Node
            DecisionNode waitWaypoint = new DecisionNode(DecisionType.SwitchStates, "Wait_Waypoint_Node");
            waitWaypoint.AddAction(new Action(idle));
            waitWaypoint.AddCondition(new Condition(ArrivedAtWaypoint));
            m_PatrolBT.AddDecisionNodeTo(patrolNode, waitWaypoint);
            m_PatrolBT.AddDecisionNodeTo(waitWaypoint, patrolNode); // node to loop back to patrolling.
        }

        private void CreateDeadBT()
        {
            /// Dead_Root_Node                (Does nothing)
            ///     

            /// Dead_Root_Node
            DecisionNode deadNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "Dead_Root_Node");
            deadNode.AddAction(new Action(idle));

            m_DeadBT = new BehaviorTree(deadNode, this, "Dead BT");
        }

        private void UpdateFactors()
        {
            if(m_State == EnemyAIState.DEAD)
            {
                return;
            }

            /* only look for threats when not attacking, because if we are attacking then
             * we dont want to look for other targets. */
            if(m_State != EnemyAIState.ATTACKING)
            {
                // if we have a target
                if (m_Target != null)
                {
                    SetMainState(EnemyAIState.CHASING);
                    /* Just in case a timer was interrupted when we switch states.*/
                    m_IsTimerSet = false;
                }

                // get highest threat.
                AIDetectable possibleHighestThreat = (DetectionManager.Instance.GetHighestThreat(this.gameObject, m_AIAudioDetection, m_AILineOfSightDetection));
                // if no threat detected
                if(possibleHighestThreat == null)
                {
                    // before nullifying our target make sure it is out of detection range.
                    if (!IsTargetInTrackingRange())
                    {
                        // ok our original target is out of range so nullify it.
                        m_Target = possibleHighestThreat;
                    }
                }
                else
                {
                    // update target to highest priority threat.
                    m_Target = possibleHighestThreat;
                }
            }

            // update factors if our current target isnt null.
            if (m_Target != null)
            {
                // if target is in attack range then attack that dude!
                Vector3 vToTarget = (m_Target.transform.position - this.transform.position);
                m_IsTargetInAttackRange = vToTarget.sqrMagnitude <= 
                    (m_AttackRange * m_AttackRange);

                // keep track of last seen position.
                m_TargetLastPosition = m_Target.transform.position;

                // keep track of target's last velcoty
                Rigidbody rigidBody = m_Target.GetComponent<Rigidbody>();
                if (rigidBody != null)
                {
                    m_TargetLastVelocity = rigidBody.velocity;
                }
            }
        }

        public override void Update()
        {
            // Calling base update ends up calling UpdateStateMachine() in parent's parent.
            UpdateStateMachine();

            /* traverse current behavior tree */
			//if(m_CurrentBT != null)
			//{
				m_CurrentBT.ContinueBehaviorTree();
			//}
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

                case EnemyAIState.DEAD:
                    m_CurrentBT = m_DeadBT;
                    break;

                default:
                    Debug.Log("Error: EnemyAISM.cs : No Behavior Tree to switch to for desired new state.");
                    break;


            }

            m_CurrentBT.RestartTree();
        }

        private bool IsTargetInTrackingRange()
        {
            if (m_Target)
            {
                return (m_Target.transform.position - this.transform.position).sqrMagnitude <= (m_TrackDistance * m_TrackDistance);
            }
            return false;
        }

        public bool IsAgrod()
        {
            return ( (m_State == EnemyAIState.ATTACKING || m_State == EnemyAIState.CHASING) || GetComponent<HealthCare.Health>().IsDead);
        }

        public void AgroToTarget(AIVisible visComp)
        {
            m_Target = visComp;
            //SetMainState(EnemyAIState.ATTACKING);
            SetMainState(EnemyAIState.CHASING);
        }

        public void Kill()
        {
            m_NavAgent.enabled = false; // don't allow movement when dead.
            this.enabled = false; // don't allow any updating in this class to happen when dead.
            if (m_Animator != null)
            {
                if (m_AudioContainer != null)
                {
                    m_AudioContainer.PlaySound(3);
                }
                m_Animator.SetBool("Dead", true);
                Destroy(gameObject, 2.0f);
            }
            SetMainState(EnemyAIState.DEAD); // this state doesnt really do anything as of now.
        }

        private void OnDrawGizmos()
        {
            if (m_Target)
            {
                Debug.DrawLine(m_AILineOfSightDetection.Apex.position, m_Target.transform.position, Color.red);
            }
        }

		private void SetAudioIsWalking(bool b)
		{
			if (m_WalkingAudioSource != null)
			{
				m_WalkingAudioSource.enabled = b;
			}
			else
			{
				Debug.Log("No Audio Source on enemy.");
			}
		}
    }
}