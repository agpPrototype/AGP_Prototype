using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MalbersAnimations;
using Wolf;
using Player;

namespace AI
{
    

    #region Wolf State Enums
    public enum WolfMainState
    {
        Idle,
        Follow,
        Alert,
        Stealth,
        Attack
    }

    public enum WolfIdleSubState
    {
        SniffAround,
        Howl,
        Wander,
        Circle,
        Sit,
        StareAtPlayer
    }

    public enum WolfFollowSubState
    {
        FollowBehind,
        FollowAlongside,
        Lead
    }

    public enum WolfAlertSubState
    {
        Growl,
        Search,
        Wait,
        Whine

    }

    public enum WolfSteatlhSubState
    {
        Stalk,
        Wait,
        Pace
    }

    public enum WolfAttackSubState
    {
        // For another day
    }


    #endregion

    public class CompanionAISM : AIStateMachine
    {

        #region Variables
        private WolfMoveComponent m_WolfMoveComp;
        private Vector3[] m_Corners;

        private StealthNavigation m_StealthNav;
        #endregion

        #region Accalia Main States

        [SerializeField]
        private WolfMainState m_CurrentMainState;

        public WolfMainState CurrentMainState
        {
            get { return m_CurrentMainState; }
        }

        [SerializeField]
        private WolfMainState m_PreviousMainState;

        public WolfMainState PreviousMainState
        {
            get { return m_PreviousMainState; }
        }

        [SerializeField]
        private WolfFollowSubState m_FollowState;

        public WolfFollowSubState CurrentFollowState
        {
            get { return m_FollowState; }
            set { m_FollowState = value; }
        }

        #endregion

        #region Accalia's Behavior Trees
        BehaviorTree m_CurrentBT;

        BehaviorTree m_FollowTree;
        BehaviorTree m_IdleTree;
        BehaviorTree m_AttackTree;
        BehaviorTree m_StealthTree;

        #endregion


        [SerializeField]
        public GameObject Player;

        private Vector3 playerLoc;

        public GameObject Enemy;

        [SerializeField]
        private Float DistToPlayerSq;


        #region Temp Variables For Testing (To go in another file later)

        private NavMeshAgent WolfNavAgent;
        private Vector3 TargetMoveToLocation;

        [SerializeField]
        private float FollowDistance;

        [SerializeField]
        private float StartToFollowDistance;

        [SerializeField]
        private Int testConditionValue;

        [SerializeField]
        bool switchToAttack;

        
        


        #endregion


        // Use this for initialization
        void Start()
        {
            PlayerControl playerControl = FindObjectOfType<PlayerControl>();
            if (playerControl)
            {
                Player = playerControl.gameObject;
            }


            m_CurrentMainState =  WolfMainState.Idle;
            m_PreviousMainState = WolfMainState.Attack;
            WolfNavAgent = GetComponentInParent<NavMeshAgent>();
            //testConditionValue = new Int(0);
            //Int testRight = new Int(1);


            //cond2 = new Condition(testConditionValue, ConditionComparison.Equal, testRight);

            //printMe = new Action(new VoidTypeDelegate(TestActionPrintFunc));

            playerLoc = Player.transform.position;
            DistToPlayerSq = new Float((playerLoc - transform.position).sqrMagnitude);

            // TEMP
            //SetMainState(WolfMainState.Stealth);


            // Get References to key components
            m_WolfMoveComp = GetComponent<WolfMoveComponent>();
            m_Corners = new Vector3[1];

            m_StealthNav = GetComponent<StealthNavigation>();

            InitializeStateBehaviorTrees();


            //StartCoroutine(waitFiveSeconds());
        }

        private void InitializeStateBehaviorTrees()
        {
            CreateIdleBT();
            CreateFollowBT();
            CreateAttackBT();
            CreateStealthBT();

            // Start in Idle state
            m_CurrentBT = m_IdleTree;
        }

        #region Create BehaviorTree Functions
        private void CreateFollowBT()
        {
            /// NODE ///
            // 
            DecisionNode rootNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "RootFollow");
            rootNode.AddAction(new Action(FollowBehind));

            m_FollowTree = new BehaviorTree(WolfMainState.Follow, rootNode, this);

            /// NODE ///
            // Node to decide when to stop following the player (if close enough to player)
            // if DistToPlayer < FollowDist, switch to Idle state
            DecisionNode stopFollowingNode = new DecisionNode(DecisionType.SwitchStates, "StopFollow->Idle");
            //Condition sfCond1 = new Condition(DistToPlayerSq, ConditionComparison.Less, new Float(FollowDistance * FollowDistance));
            Condition sfCond1 = new Condition(GetDistToPlayerSq, ConditionComparison.Less, new Float(FollowDistance * FollowDistance));

            Action switchToIdleAction = new Action(SetMainState, WolfMainState.Idle);

            stopFollowingNode.AddCondition(sfCond1);
            stopFollowingNode.AddAction(switchToIdleAction);

            // Add new Node to parent
            m_FollowTree.AddDecisionNodeTo(rootNode, stopFollowingNode);
            
            /*
            /// NODE ///
            // 
            DecisionNode rootNode = new DecisionNode(DecisionType.RepeatUntilActionComplete, "RootFollow");
            rootNode.AddAction(new Action(FollowBehind));

            m_FollowTree = new BehaviorTree(WolfMainState.Follow, rootNode, this);

            /// NODE ///
            // Node to decide when to stop following the player (if close enough to player)
            // if DistToPlayer < FollowDist, switch to Idle state
            DecisionNode stopFollowingNode = new DecisionNode(DecisionType.SwitchStates, "StopFollow->Idle");
            Action switchToIdleAction = new Action(SetMainState, WolfMainState.Idle);

            stopFollowingNode.AddAction(switchToIdleAction);

            // Add new Node to parent
            m_FollowTree.AddDecisionNodeTo(rootNode, stopFollowingNode);
            */
        }

        private void CreateIdleBT()
        {
            /// NODE ///
            // For now, idle will just do nothing
            DecisionNode rootNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "RootIdle");
            rootNode.AddAction(new Action(DoNothing));

            m_IdleTree = new BehaviorTree(WolfMainState.Idle, rootNode, this);

            /// NODE ///
            // Node to decide if wolf should follow player
            // if DistToPlayer > StartToFollowDist, switch to Follow state
            DecisionNode toFollowNode = new DecisionNode(DecisionType.SwitchStates, "StartFollow->Follow");
            Condition tfCond1 = new Condition(DistToPlayerSq, ConditionComparison.Greater, new Float(StartToFollowDistance * StartToFollowDistance));
            Action switchToFollowAction = new Action(SetMainState, WolfMainState.Follow);

            toFollowNode.AddCondition(tfCond1);
            toFollowNode.AddAction(switchToFollowAction);

            // Add new Node to parent
            m_IdleTree.AddDecisionNodeTo(rootNode, toFollowNode);
        }

        private void CreateAttackBT()
        {
            /// NODE ///
            // 
            DecisionNode rootNode = new DecisionNode(DecisionType.RepeatUntilActionComplete, "RootAttack");
            rootNode.AddAction(new Action(DetermineTarget));

            m_AttackTree = new BehaviorTree(WolfMainState.Attack, rootNode, this);

            /// NODE ///
            // Node to decide when to stop following the player (if close enough to player)
            DecisionNode moveToTargetNode = new DecisionNode(DecisionType.RepeatUntilActionComplete, "MoveToEnemy");
            Action moveToEnemy = new Action(MoveToEnemy);
            moveToTargetNode.AddAction(moveToEnemy);

            m_AttackTree.AddDecisionNodeTo(rootNode, moveToTargetNode);

            /// NODE ///
            /// 
            DecisionNode attackEnemyNode = new DecisionNode(DecisionType.RepeatUntilActionComplete, "AttackEnemy");
            Action attackEnemy = new Action(AttackMyEnemy);
            attackEnemyNode.AddAction(attackEnemy);

            m_AttackTree.AddDecisionNodeTo(moveToTargetNode, attackEnemyNode);

            /// NODE /// 
            /// 
            DecisionNode toFollowNode = new DecisionNode(DecisionType.SwitchStates, "StopAttack->Follow");
            Action switchToFollow = new Action(SetMainState, WolfMainState.Follow);
            toFollowNode.AddAction(switchToFollow);

            m_AttackTree.AddDecisionNodeTo(attackEnemyNode, toFollowNode);


        }

        private void CreateStealthBT()
        {
            DecisionNode rootNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "Root Stealth");
            Condition isAtStealthPt = new Condition(new BoolTypeDelegate(m_StealthNav.IsAtNextNode));
            Action beginStealth = new Action(DoNothing);

            rootNode.AddCondition(isAtStealthPt);
            rootNode.AddAction(beginStealth);

            m_StealthTree = new BehaviorTree(WolfMainState.Stealth, rootNode, this);

            /// NODE ///
            /// 
            DecisionNode navigateToNextStealthPt = new DecisionNode(DecisionType.RepeatUntilCanProgress, "NavigateToStealthPt");
            Condition isPlayerAway = new Condition(new FloatTypeDelegate(GetDistToPlayerSq), ConditionComparison.Greater, new Float(StartToFollowDistance * StartToFollowDistance));
            Action navToNext = new Action(m_StealthNav.NavigateToNextNode);

            navigateToNextStealthPt.AddCondition(isPlayerAway);
            navigateToNextStealthPt.AddAction(navToNext);

            m_StealthTree.AddDecisionNodeTo(rootNode, navigateToNextStealthPt);

            // Loop back to top
            m_StealthTree.AddDecisionNodeTo(navigateToNextStealthPt, rootNode);
        }

        #endregion


        IEnumerator waitFiveSeconds()
        {
            yield return new WaitForSeconds(3);

            SetMainState(WolfMainState.Stealth);
        }

        // Update is called once per frame
        void Update()
        {

            UpdateFactors();

            // Check for events or player suggestions to switch State

            // Traverse current Behavior Tree
            if (!ReferenceEquals(m_CurrentBT, null))
                m_CurrentBT.ContinueBehaviorTree();
            else
                Debug.Log("CompanionAISM: current BT not initialized yet");


            // Determine what the current state should be
            //if (!cond2.IsMet())
            // UpdateStateMachine();

        }

        private void UpdateFactors()
        {
            DistToPlayerSq.value = (Player.transform.position - transform.position).sqrMagnitude;

            if (false && switchToAttack)
            {
                SetMainState(WolfMainState.Attack);
                switchToAttack = false;
            }
        }


        public override void UpdateStateMachine()
        {
            // Update Factors?

            // Execute current state
            //switch (CurrentMainState)
            //{
            //    case WolfMainState.Idle:
            //        ExecuteIdle();
            //        break;

            //    case WolfMainState.Follow:
            //        ExecuteFollow();
            //        break;

            //    case WolfMainState.Stealth:
            //        ExecuteStealth();
            //        break;

            //    case WolfMainState.Alert:
            //        ExecuteAlert();
            //        break;

            //    case WolfMainState.Attack:
            //        ExecuteAttack();
            //        break;
            //}
        }

        void SetMainState(WolfMainState newState)
        {
            m_PreviousMainState = m_CurrentMainState;
            m_CurrentMainState = newState;

            //m_CurrentBT.RestartTree();

            // Switch the current behavior tree to the new state's tree
            switch (m_CurrentMainState)
            {
                case WolfMainState.Idle:
                    m_CurrentBT = m_IdleTree;
                    break;

                case WolfMainState.Follow:
                    m_CurrentBT = m_FollowTree;
                    break;

                case WolfMainState.Attack:
                    m_CurrentBT = m_AttackTree;
                    break;

                case WolfMainState.Stealth:
                    m_CurrentBT = m_StealthTree;
                    m_StealthNav.ActivateStealthNavigation();
                    break;

                default:
                    Debug.Log("Error: CompanionAISM.cs : No Behavior Tree to switch to for desired new state.");
                    break;


            }

            m_CurrentBT.RestartTree();
        }

        void SetPrevousMainState(WolfMainState newState)
        {
            m_PreviousMainState = newState;
        }


        #region MovementFunctions

        public void MoveTo(Vector3 Location)
        {
            WolfNavAgent.SetDestination(Location);
            WolfNavAgent.Resume();

            // Have I reached my destination? If so, trigger action complete
            // float dstSq = (WolfNavAgent.destination - transform.position).sqrMagnitude;
            //WolfNavAgent.
            float dstSq = (Location - transform.position).sqrMagnitude;
            if (/*WolfNavAgent.velocity == Vector3.zero &&*/ dstSq < 16.0f)
            {
                CompleteCurrentActionExternal(true);
            }
        }

        public void FollowBehind()
        {
            Vector3 toPlayer = Player.transform.position - this.transform.position;
            toPlayer.Normalize();

            TargetMoveToLocation = Player.transform.position - FollowDistance * toPlayer;


            /*
             * idealy we'd move this whole calculation into move component,but lets just do this for now
             * and wait to see how much we want the AI to calculate and how much the move comp should calculate
             */
             //Executing following in the move component
            NavMeshPath path = new NavMeshPath();
            WolfNavAgent.CalculatePath(TargetMoveToLocation, path);
            if (path.corners.Length == 0)
            {
                Debug.Log("NO PATH FOUND! THIS SHOULD NOT HAPPEN!!");
            }
            
            if (path.corners.Length != 0)
            {
                if (m_Corners[0] != path.corners[0])
                {
                    m_Corners = path.corners;
                    m_WolfMoveComp.Move(TargetMoveToLocation, m_Corners);
                }
            }
        }
        #endregion

        #region Idle Functions

        public void DoNothing()
        {
            //OnActionComplete(true);
            m_WolfMoveComp.Stop();
        }

        #endregion

        #region Attack Functions

        private void MoveToEnemy()
        {
            if(!ReferenceEquals(Enemy, null))
            {
                MoveTo(Enemy.transform.position);
            }
            else
            {
                Debug.Log("Enemy is a null reference!");
            }
        }

        private void DetermineTarget()
        {
            CompleteCurrentActionExternal(true);
            //OnActionComplete(true);
        }

        private void AttackMyEnemy()
        {
            //Debug.Log("Attacking enemy!");
        }

        #endregion

        #region Utillity Functions

        float GetDistToPlayerSq()
        {
            return (Player.transform.position - transform.position).sqrMagnitude;
        }

        #endregion

        #region State Execution Functions Temp

        void ExecuteIdle()
        {
            //
        }

        void ExecuteFollow()
        {

            // Set up follow info (decide where to follow), Randomly choose for now
            if (PreviousMainState != CurrentMainState)
            {
                int followOrientation = Random.Range(0, 2);
                CurrentFollowState = (WolfFollowSubState)followOrientation;
                if (CurrentFollowState == WolfFollowSubState.FollowAlongside)
                {
                    int leftOrRight; // Randomly choose between following left or right
                    if (Random.Range(0, 2) == 0)
                    {
                        leftOrRight = -1;
                    }
                    else
                    {
                        leftOrRight = 1;
                    }
                }
            }

            // temp
            CurrentFollowState = WolfFollowSubState.FollowBehind;
            Vector3 playerLocation = Player.transform.position;

            switch (CurrentFollowState)
            {


                case WolfFollowSubState.FollowBehind: // maybe make seperate functions for different sub states
                    Vector3 toPlayer = playerLocation - this.transform.position;
                    toPlayer.Normalize();

                    TargetMoveToLocation = playerLocation - FollowDistance * toPlayer;
                    Animal an = GetComponentInParent<Animal>();

                    //if(an != null)
                    //   an.Move(playerLocation, false);
                    //WolfNavAgent.SetDestination(TargetMoveToLocation);
                    break;

                case WolfFollowSubState.FollowAlongside:



                    break;

            }

        }
        #endregion 


        public void TestActionPrintFunc()
        {
            Debug.Log("Action delegate worked!!!");
        }
    }

}