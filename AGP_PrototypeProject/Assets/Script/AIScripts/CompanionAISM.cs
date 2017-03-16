using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MalbersAnimations;
using Wolf;
using Player;
using Bond;

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

    #region Wolf Command Enums
    public enum WolfCommand{
        GOTO,
        STAY,
        COME
    }

    #endregion

    public class CompanionAISM : AIStateMachine
    {

        #region Variables
        private WolfMoveComponent m_WolfMoveComp;
        private Vector3[] m_Corners;

        private StealthNavigation m_StealthNav;

        private GameCritical.GameController m_GameControl;
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

        

        [SerializeField]
        private Float DistToPlayerSq;

        private Vector3 m_RotateToDirection;


        #region Temp Variables For Testing (To go in another file later)

        private NavMeshAgent WolfNavAgent;
        private Vector3 TargetMoveToLocation;

        [SerializeField]
        private float FollowDistance;

        [SerializeField]
        private float StartToFollowDistance;

        [SerializeField]
        private float m_MaxTimeToWaitAtStealthPoint;
        private bool m_WaitStealthTimerSet = false;
        private float m_WaitStealthEndTime;

        private bool m_DidPlayerLeaveZone = false;

        [SerializeField]
        bool switchToAttack;

        [SerializeField]
        public GameObject Enemy;

        [SerializeField]
        public StealthPosition StealthDestination;

        #endregion


        // Use this for initialization
        void Start()
        {
            m_GameControl = GameCritical.GameController.Instance;

            PlayerControl playerControl = FindObjectOfType<PlayerControl>();
            if (playerControl)
            {
                Player = playerControl.gameObject;
            }


            m_CurrentMainState =  WolfMainState.Idle;
            m_PreviousMainState = WolfMainState.Attack;
            WolfNavAgent = GetComponentInParent<NavMeshAgent>();

            playerLoc = Player.transform.position;
            DistToPlayerSq = new Float((playerLoc - transform.position).sqrMagnitude);

            m_RotateToDirection = Vector3.zero;

            // Get References to key components
            m_WolfMoveComp = GetComponent<WolfMoveComponent>();
            m_Corners = new Vector3[1];

            m_StealthNav = GetComponent<StealthNavigation>();

            InitializeStateBehaviorTrees();

            m_GameControl.RegisterWolf(gameObject);
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

            //GetComponent<Animator>().SetBool("Attack1", true);
        }

        private void CreateStealthBT()
        {
            ///                              root
            /// (FollowStealth)                            (FindOwnPath)
            /// FindNextStealthPt                           FindPathToEnd
            /// GoToNextStealthPt                           GoToNextStealthPt
            /// RotateTowardsNext (once arrived)            RotateTowardsNext (once arrived)
            /// LoopToRoot                                  Wait
            ///                                             LoopTo"GoToNextStealthPt"
            ///                                             
            DecisionNode rootNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "Root Stealth");
            //Condition isAtStealthPt = new Condition(new BoolTypeDelegate(m_StealthNav.IsAtNextNode));
            Action beginStealth = new Action(DoNothing);

            // Test Go To stealth navigation
            rootNode.AddAction(beginStealth);

            m_StealthTree = new BehaviorTree(WolfMainState.Stealth, rootNode, this);

            #region StealthLeftTree_Follow

            /// NODE ///
            /// 
            DecisionNode findNextStealthPt = new DecisionNode(DecisionType.RepeatUntilCanProgress, "FindNextStealthPt");
            Condition isBondLow1 = new Condition(new FloatTypeDelegate(BondManager.Instance.GetBondStatus), ConditionComparison.Greater, new Float(50.0f));
            Action getNextPt = new Action(m_StealthNav.DetermineNextStealthPointToPlayer);
            findNextStealthPt.AddCondition(isBondLow1);
            findNextStealthPt.AddAction(getNextPt);

            m_StealthTree.AddDecisionNodeTo(rootNode, findNextStealthPt);
            
            /// NODE ///
            /// 
            DecisionNode navigateToNextStealthPt = new DecisionNode(DecisionType.RepeatUntilCanProgress, "NavigateToStealthPt");
            Condition isPlayerAway = new Condition(new FloatTypeDelegate(GetDistToPlayerSq), ConditionComparison.Greater, new Float(StartToFollowDistance * StartToFollowDistance));
            // Condition isBondHigh = new Condition(new FloatTypeDelegate(BondManager.Instance.GetBondStatus), ConditionComparison.Greater, new Float(50.0f));
            Condition isPathSafe = new Condition(new BoolTypeDelegate(m_StealthNav.IsPathSafeToNext));
            Action navToNext = new Action(m_StealthNav.NavigateToNextNode);

            // navigateToNextStealthPt.AddCondition(isBondHigh);
            navigateToNextStealthPt.AddCondition(isPathSafe);
            navigateToNextStealthPt.AddCondition(isPlayerAway);
            navigateToNextStealthPt.AddAction(navToNext);

            m_StealthTree.AddDecisionNodeTo(findNextStealthPt, navigateToNextStealthPt);

            /// NODE ///
            /// 
            DecisionNode rotateTowardsNext = new DecisionNode(DecisionType.RepeatUntilActionComplete, "RotateToNext");
            Condition isAtStealthPt = new Condition(new BoolTypeDelegate(m_StealthNav.IsAtNextNode));
            //Action rotateWolf = new Action(RotateTowardsEnemyPath);
            Action rotateWolf = new Action(RotateTowardsNextStealthPos);

            rotateTowardsNext.AddCondition(isAtStealthPt);
            rotateTowardsNext.AddAction(rotateWolf);
            m_StealthTree.AddDecisionNodeTo(navigateToNextStealthPt, rotateTowardsNext);

            // Loop back to top
            m_StealthTree.AddDecisionNodeTo(rotateTowardsNext, rootNode);


            #endregion

            #region StealthRightTree_GoOffOnOwn

            /// NODE ///
            /// 
            DecisionNode findNextStealthPt_path = new DecisionNode(DecisionType.RepeatUntilCanProgress, "FindNextStealthPt");
            Action getNextPt2 = new Action(m_StealthNav.DetermineNextStealthPointInPath);
            findNextStealthPt_path.AddAction(getNextPt2);

            m_StealthTree.AddDecisionNodeTo(rootNode, findNextStealthPt_path);

            /// NODE ///
            /// 
            DecisionNode navToNextStealthPt_path = new DecisionNode(DecisionType.RepeatUntilCanProgress, "NavigateToStealthPt");
            Condition isPathSafe_path = new Condition(new BoolTypeDelegate(m_StealthNav.IsPathSafeToNext));
            Action navToNext_path = new Action(m_StealthNav.NavigateToNextNode);
            navToNextStealthPt_path.AddCondition(isPathSafe_path);
            navToNextStealthPt_path.AddAction(navToNext_path);

            m_StealthTree.AddDecisionNodeTo(findNextStealthPt_path, navToNextStealthPt_path);

            /// NODE ///
            /// 
            DecisionNode rotateTowardsNextSP = new DecisionNode(DecisionType.RepeatUntilActionComplete, "RotateToNext");
            Condition isAtStealthPt2 = new Condition(new BoolTypeDelegate(m_StealthNav.IsAtNextNode));
            Action rotateWolf2 = new Action(RotateTowardsNextStealthPos);
            rotateTowardsNextSP.AddCondition(isAtStealthPt2);
            rotateTowardsNextSP.AddAction(rotateWolf2);

            m_StealthTree.AddDecisionNodeTo(navToNextStealthPt_path, rotateTowardsNextSP);


            /// NODE ///
            /// 
            DecisionNode doNothingNode0  = new DecisionNode(DecisionType.RepeatUntilCanProgress, "WaitAtStealthPoint");
            Action doNothing = new Action(DoNothing);
            doNothingNode0.AddAction(doNothing);

            m_StealthTree.AddDecisionNodeTo(rotateTowardsNextSP, doNothingNode0);

            /// NODE ///
            /// 
            DecisionNode waitHere = new DecisionNode(DecisionType.RepeatUntilCanProgress, "WaitForSafePath");
            Condition isWaitDone = new Condition(IsDoneWaitAtStealthPoint);
            waitHere.AddAction(doNothing);
            waitHere.AddCondition(isWaitDone);

            m_StealthTree.AddDecisionNodeTo(doNothingNode0, waitHere);

            /// NODE ///
            /// 
            DecisionNode doNothingNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "DoNothing");
            Condition isPathComplete = new Condition(new BoolTypeDelegate(m_StealthNav.IsStealthPathComplete));
           // Action doNothing = new Action(DoNothing);
            doNothingNode.AddCondition(isPathComplete);
            doNothingNode.AddAction(doNothing);

            m_StealthTree.AddDecisionNodeTo(waitHere, doNothingNode);

            // Loop back to "nav to next" if not at end of path
            m_StealthTree.AddDecisionNodeTo(waitHere, findNextStealthPt_path);

            #endregion
        }

        #endregion


        IEnumerator waitForTime(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);

            m_WaitStealthTimerSet = false;
            CompleteCurrentActionExternal(true);
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
                Debug.Assert(false, "CompanionAISM: current BT not initialized yet");
        }

        private void UpdateFactors()
        {
            DistToPlayerSq.value = (Player.transform.position - transform.position).sqrMagnitude;


            // Determine if Stealth is correct state to be in
            bool isPlayerStealthed = Player.GetComponent<MoveComponent>().m_Crouching;
            if (isPlayerStealthed && m_CurrentMainState != WolfMainState.Stealth && m_GameControl.CurrentActionZone)
            {
                SetMainState(WolfMainState.Stealth);
                Debug.Log("Accalia Switched to Stealth state");

                return;
            }

            // See if player is out of action zone and follow if 
            if (m_DidPlayerLeaveZone && m_GameControl.CurrentActionZone) { 

                if (m_GameControl.CurrentActionZone.IsAtFinalStealthPoint(transform.position))
                {
                    SetMainState(WolfMainState.Follow);
                    return;
                }
                else
                {
                    m_StealthNav.FindOwnPath = true;
                    SetMainState(WolfMainState.Stealth);
                    return;
                }

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

        public void SetMainState(WolfMainState newState)
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

        #region Command Functions

        /// <summary>
        /// Call this for Commands other than "GoTo"
        /// </summary>
        public void GiveCommand(WolfCommand playerCommand)
        {

        }

        /// <summary>
        /// For a "GoTo" command: Pass in the game object that was hit from the raycast (to check if it was an enemy),
        /// as well as the location that the ray cast hit (to tell the wolf to actually go to that location
        /// </summary>
        public void GiveGoToCommand(GameObject hitObject, Vector3 goToLocation)
        {

        }

        #endregion

        #region Movement Functions

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

        #region StealthFunctions

        public void RotateTowardsNextStealthPos()
        {
            if (m_StealthNav.CurrentStealthPos)
            {
                Vector3 faceDir = m_StealthNav.CurrentStealthPos.GetComponent<StealthPosition>().AlignmentDir;

                if (m_WolfMoveComp.RotateTowards(faceDir))
                {
                    m_WolfMoveComp.Stop();
                    CompleteCurrentActionExternal(true);
                }
            }
            else {
                m_WolfMoveComp.Stop();
                CompleteCurrentActionExternal(true);
            }

        }

        public void RotateTowardsEnemyPath()
        {
            if (m_RotateToDirection == Vector3.zero)
            {
                Vector3 EnemyPos = Enemy.transform.position;
                Vector3 EnemyDir = Enemy.GetComponent<NavMeshAgent>().destination - EnemyPos;
                EnemyDir.Normalize();

                Vector3 toWolfFromEnemy = transform.position - EnemyPos;
                toWolfFromEnemy.Normalize();

                float lengthUntilPathsIntersect = Vector3.Dot(toWolfFromEnemy, EnemyDir);
                m_RotateToDirection = EnemyPos + EnemyDir * lengthUntilPathsIntersect;
            }

            if (m_WolfMoveComp.RotateTowards(m_RotateToDirection))
            {
                m_RotateToDirection = Vector3.zero;
                CompleteCurrentActionExternal(true);
            }
        }


        public bool IsDoneWaitAtStealthPoint()
        {
            if (!m_WaitStealthTimerSet)
            {
                m_WaitStealthTimerSet = true;
                m_WaitStealthEndTime = Time.time + m_MaxTimeToWaitAtStealthPoint;
            }

            if(Time.time < m_WaitStealthEndTime)
            {
                return false;
            }

            m_WaitStealthTimerSet = false;
            return true;
        }

        public void PlayerLeftActionZone(bool didPlayerLeave)
        {
            //if (GameCritical.GameController.Instance.CurrentActionZone.IsAtFinalStealthPoint(gameObject.transform.position))
            //{
            //    SetMainState(WolfMainState.Follow);
            //}
            m_DidPlayerLeaveZone = didPlayerLeave;
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