using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MalbersAnimations;
using Wolf;
using Player;
using Bond;
using HealthCare;
using Audio;

namespace AI
{

    #region Wolf State Enums
    public enum WolfMainState
    {
        Idle,
        Follow,
        Alert,
        Stealth,
        Attack,
        Stay
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
        COME,
        NONE
    }

    #endregion

    public class CompanionAISM : AIStateMachine
    {

        #region Variables
        private WolfMoveComponent m_WolfMoveComp;
        private Animator m_Animator;
        private AudioContainer m_AudioContainer;
        private AccaliaHealth m_Health;
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
        BehaviorTree m_StayTree;

        #endregion


        [SerializeField]
        public GameObject Player;

        private Vector3 playerLoc;

        

        [SerializeField]
        private Float DistToPlayerSq;

        private Vector3 m_RotateToDirection;


        #region Command Vars

        private WolfCommand m_CurrentCommand;
        public WolfCommand CurrentCommand { get { return m_CurrentCommand; } }

        public Int m_CurrentCommandAsInt;

        [SerializeField]
        private float m_GoToBondRequirement;
        [SerializeField]
        private float m_StayBondRequirement;
        [SerializeField]
        private float m_ComeBondRequirement;

        [SerializeField]
        private float m_MaxTimeToStay;
        private bool m_StayTimerSet = false;
        private float m_StayEndTime;

        #endregion

        #region StateMachine Vars

        private NavMeshAgent WolfNavAgent;
        private Vector3 TargetMoveToLocation;

        [SerializeField]
        private float FollowDistance;

        [SerializeField]
        private float StartToFollowDistance;

        [SerializeField]
        private float m_BreakStealthToAttackThreshold;

        [SerializeField]
        private float m_MaxTimeToWaitAtStealthPoint;
        private bool m_WaitStealthTimerSet = false;
        private float m_WaitStealthEndTime;

        private bool m_DidPlayerLeaveZone = false;

        private GameObject m_EnemyTarget;
        private GameObject m_PlayersEnemyTarget;

        [SerializeField]
        private float m_AttackRange;
        [SerializeField]
        private float m_AttackCooldown;
        private float m_AttackCooldownEndTime;
        private bool m_AttackTimerSet = false;

        private bool m_IsAttacking = false;


        [SerializeField]
        private float m_IdleAnimCooldownMax;
        [SerializeField]
        private float m_IdleAnimCooldownMin;
        private float m_IdleCooldownEndTime;
        private bool m_IdleTimerSet = false;

        [SerializeField]
        private float m_IdleSitTime;
        [SerializeField]
        private float m_IdleSleepTime;
        private float m_IdleStateStartTime;

        [SerializeField]
        public StealthPosition StealthDestination;

        private bool m_SkipBTUpdate = false;
        #endregion

        void Awake()
        {
            
        }
        // Use this for initialization
        void Start()
        {
            // m_GameControl = GameCritical.GameController.Instance;
            m_GameControl = GameCritical.GameController.Instance;
            m_GameControl.RegisterWolf(gameObject);

            PlayerControl playerControl = FindObjectOfType<PlayerControl>();
            if (playerControl)
            {
                Player = playerControl.gameObject;
            }


            m_CurrentMainState =  WolfMainState.Idle;
            m_PreviousMainState = WolfMainState.Idle;
            WolfNavAgent = GetComponentInParent<NavMeshAgent>();
            m_Animator = GetComponent<Animator>();
            m_AudioContainer = GetComponent<AudioContainer>();

            m_CurrentCommand = WolfCommand.NONE;

            playerLoc = Player.transform.position;
            DistToPlayerSq = new Float((playerLoc - transform.position).sqrMagnitude);

            m_RotateToDirection = Vector3.zero;

            // Get References to key components
            m_WolfMoveComp = GetComponent<WolfMoveComponent>();
            m_Corners = new Vector3[1];

            m_StealthNav = GetComponent<StealthNavigation>();

            InitializeStateBehaviorTrees();

            //m_GameControl.RegisterWolf(gameObject);

            m_Health = GetComponent<AccaliaHealth>();
        }

        private void InitializeStateBehaviorTrees()
        {
            CreateIdleBT();
            CreateFollowBT();
            CreateAttackBT();
            CreateStealthBT();
            CreateStayBT();

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
            rootNode.AddAction(new Action(StartIdle));

            m_IdleTree = new BehaviorTree(WolfMainState.Idle, rootNode, this);

            /// NODE ///
            /// 
            DecisionNode doNothingNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "DoNothing");
            doNothingNode.AddAction(new Action(DoNothing));

            /// NODE///
            /// 
            DecisionNode endIdleNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "EndIdleVars");
            Condition tfCond1 = new Condition(DistToPlayerSq, ConditionComparison.Greater, new Float(StartToFollowDistance * StartToFollowDistance));
            Action endIdleVar = new Action(EndIdleState);
            endIdleNode.AddCondition(tfCond1);
            endIdleNode.AddAction(endIdleVar);

            /// NODE ///
            // Node to decide if wolf should follow player
            // if DistToPlayer > StartToFollowDist, switch to Follow state
            DecisionNode toFollowNode = new DecisionNode(DecisionType.SwitchStates, "StartFollow->Follow");
            Action switchToFollowAction = new Action(SetMainState, WolfMainState.Follow);
            toFollowNode.AddAction(switchToFollowAction);

            /// NODE ///
            /// 
            DecisionNode doIdleAnim = new DecisionNode(DecisionType.RepeatUntilCanProgress, "RandomIdleAnim");
            Condition idleTimerCond = new Condition(IsIdleTimerDone);
            Action playIdleAnim = new Action(PlayRandomIdleAnimation);

            doIdleAnim.AddAction(playIdleAnim);
            doIdleAnim.AddCondition(idleTimerCond);

            // Add new Node to parent
            m_IdleTree.AddDecisionNodeTo(rootNode, doNothingNode);
            m_IdleTree.AddDecisionNodeTo(doNothingNode, endIdleNode);
            m_IdleTree.AddDecisionNodeTo(endIdleNode, toFollowNode);

            m_IdleTree.AddDecisionNodeTo(doNothingNode, doIdleAnim);

            // Loop Back after idle anim
            m_IdleTree.AddDecisionNodeTo(doIdleAnim, doNothingNode);
        }

        private void CreateAttackBT()
        {
            /// NODE ///
            // 
            DecisionNode rootNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "RootAttack");
            rootNode.AddAction(new Action(DetermineTarget));

            m_AttackTree = new BehaviorTree(WolfMainState.Attack, rootNode, this);

            /// NODE ///
            // Node to decide when to stop following the player (if close enough to player)
            DecisionNode moveToTargetNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "MoveToEnemy");
            Action moveToEnemy = new Action(MoveToEnemy);
            moveToTargetNode.AddAction(moveToEnemy);

            m_AttackTree.AddDecisionNodeTo(rootNode, moveToTargetNode);

            /// NODE ///
            /// 
            DecisionNode attackEnemyNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "AttackEnemy");
            Condition isAtkTimerDone = new Condition(new BoolTypeDelegate(IsAttackTimerDone));
            Condition isAtEnemy = new Condition(new BoolTypeDelegate(IsAtEnemyTarget));
            Action attackEnemy = new Action(AttackMyEnemy);
            attackEnemyNode.AddAction(attackEnemy);
            attackEnemyNode.AddCondition(isAtkTimerDone);
            attackEnemyNode.AddCondition(isAtEnemy);

            m_AttackTree.AddDecisionNodeTo(moveToTargetNode, attackEnemyNode);

            // Loop back to root
            m_AttackTree.AddDecisionNodeTo(attackEnemyNode, rootNode);

            /// NODE /// 
            /// 
            //DecisionNode toRevertState = new DecisionNode(DecisionType.SwitchStates, "StopAttack->GoToPrevState");
            //Action switchToFollow = new Action(SetMainState, WolfMainState.Follow);
            //toRevertState.AddAction(switchToFollow);

           // m_AttackTree.AddDecisionNodeTo(attackEnemyNode, toRevertState);

            //GetComponent<Animator>().SetBool("Attack1", true);
        }

        private void CreateStealthBT()
        {
            ///                                              /root\
            /// (FollowStealth)                                                                   (FindOwnPath)
            /// FindNextStealthPt                                                                  FindPathToEnd
            ///                                                                           /                       \
            //  GoToNextStealthPt                                     GoToNextStealthPt               <--->        WaitForTime
            /// RotateTowardsNext (once arrived)                      RotateTowardsNext (once arrived)
            /// LoopToRoot                                            Wait
            ///                                                       LoopTo"GoToNextStealthPt"
            ///                                             
            DecisionNode rootNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "Root Stealth");
            Action beginStealth = new Action(DoNothing);

            // Test Go To stealth navigation
            rootNode.AddAction(beginStealth);

            m_StealthTree = new BehaviorTree(WolfMainState.Stealth, rootNode, this);

            #region ComeViaStealth_Come

            DecisionNode findNextStealthPt_come = new DecisionNode(DecisionType.RepeatUntilCanProgress, "FindNextStealthPt_come");
            Condition isCommandCome = new Condition(new BoolTypeDelegate(IsCommandCome));
            Action getNextPt_come = new Action(m_StealthNav.DetermineNextStealthPointInPath);
            findNextStealthPt_come.AddCondition(isCommandCome);
            findNextStealthPt_come.AddAction(getNextPt_come);

            DecisionNode endComeNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "EndComeCommand");
            Condition isAtLast = new Condition(new BoolTypeDelegate(m_StealthNav.IsAtNextNode));
            Action endCome = new Action(EndCommand);
            endComeNode.AddCondition(isAtLast);
            endComeNode.AddAction(endCome);

            m_StealthTree.AddDecisionNodeTo(findNextStealthPt_come, endComeNode);
            m_StealthTree.AddDecisionNodeTo(endComeNode, rootNode);

            #endregion

            #region StealthLeftTree_Follow

            /// NODE ///
            /// 
            DecisionNode findNextStealthPt = new DecisionNode(DecisionType.RepeatUntilCanProgress, "FindNextStealthPt");
            Condition isBondHigh1 = new Condition(new FloatTypeDelegate(BondManager.Instance.GetBondStatus), ConditionComparison.Greater, new Float(50.0f));
            Condition isNotUnderCommand = new Condition(new BoolTypeDelegate(IsNotUnderCommand));
            Action getNextPt = new Action(m_StealthNav.DetermineNextStealthPointToPlayer);
            findNextStealthPt.AddCondition(isBondHigh1);
            findNextStealthPt.AddCondition(isNotUnderCommand);
            findNextStealthPt.AddAction(getNextPt);

            m_StealthTree.AddDecisionNodeTo(rootNode, findNextStealthPt);

            

            /// NODE ///
            /// 
            DecisionNode navigateToNextStealthPt = new DecisionNode(DecisionType.RepeatUntilCanProgress, "NavigateToStealthPt");
            Condition isPlayerAway = new Condition(new FloatTypeDelegate(GetDistToPlayerSq), ConditionComparison.Greater, new Float(StartToFollowDistance * StartToFollowDistance));
            Condition isPathSafe = new Condition(new BoolTypeDelegate(m_StealthNav.IsPathSafeToNext));
            Action navToNext = new Action(m_StealthNav.NavigateToNextNode);
            navigateToNextStealthPt.AddCondition(isPathSafe);
            navigateToNextStealthPt.AddCondition(isPlayerAway);
            navigateToNextStealthPt.AddAction(navToNext);

            m_StealthTree.AddDecisionNodeTo(findNextStealthPt, navigateToNextStealthPt);

            //Link "Come" branch to this
            //m_StealthTree.AddDecisionNodeTo(findNextStealthPt_come, navigateToNextStealthPt);

                //Only Wait loop if path is not safe (give the ai some time to get out of the way
                    /// NODE ///
                    /// 
                    DecisionNode doNothingNode1 = new DecisionNode(DecisionType.RepeatUntilCanProgress, "WaitAtStealthPoint");
                    Action doNothing0 = new Action(DoNothing);
                    doNothingNode1.AddAction(doNothing0);

                    m_StealthTree.AddDecisionNodeTo(findNextStealthPt, doNothingNode1);

                    /// NODE ///
                    /// 
                    DecisionNode waitHere0 = new DecisionNode(DecisionType.RepeatUntilCanProgress, "WaitForSafePath");
                    Condition isWaitDone0 = new Condition(IsDoneWaitAtStealthPoint);
                    waitHere0.AddAction(doNothing0);
                    waitHere0.AddCondition(isWaitDone0);

                    m_StealthTree.AddDecisionNodeTo(doNothingNode1, waitHere0);

                    // Add loop to wait again if path is not safe
                    m_StealthTree.AddDecisionNodeTo(waitHere0, navigateToNextStealthPt);
                    m_StealthTree.AddDecisionNodeTo(waitHere0, doNothingNode1);

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

            // m_StealthTree.AddDecisionNodeTo(rootNode, findNextStealthPt_path);

            /// NODE ///
            /// 
            DecisionNode switchToAttackNode = new DecisionNode(DecisionType.SwitchStates, "SwitchToAttack");
            Condition shouldBreakStealth = new Condition(ToBreakStealthForAttack);
            Action switchToAttack = new Action(SetMainState, WolfMainState.Attack);
            switchToAttackNode.AddCondition(shouldBreakStealth);
            switchToAttackNode.AddAction(switchToAttack);

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

            //Check if should break stealth before moving to next Node
            m_StealthTree.AddDecisionNodeTo(rotateTowardsNextSP, switchToAttackNode);

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
            doNothingNode.AddCondition(isPathComplete);
            doNothingNode.AddAction(doNothing);

            m_StealthTree.AddDecisionNodeTo(waitHere, doNothingNode);

            //Check if should break stealth before moving to next Node
            //m_StealthTree.AddDecisionNodeTo(waitHere, switchToAttackNode);

            // Loop back to "nav to next" if not at end of path
            m_StealthTree.AddDecisionNodeTo(waitHere, findNextStealthPt_path);

            #endregion


            m_StealthTree.AddDecisionNodeTo(rootNode, findNextStealthPt_come);
            //Link "Come" branch to this
            m_StealthTree.AddDecisionNodeTo(findNextStealthPt_come, navigateToNextStealthPt);
            
            m_StealthTree.AddDecisionNodeTo(rootNode, findNextStealthPt);
            m_StealthTree.AddDecisionNodeTo(rootNode, findNextStealthPt_path);
        }

        private void CreateStayBT()
        {

            DecisionNode rootNode = new DecisionNode(DecisionType.RepeatUntilCanProgress, "Root Stay");
            Action doNothing = new Action(DoNothing);
            rootNode.AddAction(doNothing);

            m_StayTree = new BehaviorTree(WolfMainState.Stay, rootNode, this);

            DecisionNode endStay = new DecisionNode(DecisionType.SwitchStates, "SwitchToPrevState");
            Condition stayTimer = new Condition(IsStayTimerDone);
            Action switchToPrevState = new Action(SetMainState, m_PreviousMainState);
            endStay.AddCondition(stayTimer);
            endStay.AddAction(switchToPrevState);
            m_StayTree.AddDecisionNodeTo(rootNode, endStay);
        }

        #endregion


        IEnumerator waitForTime(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);

            m_WaitStealthTimerSet = false;
            CompleteCurrentActionExternal(true);
        }

        #region Command Functions

        public bool IsNotUnderCommand()
        {
            if (m_CurrentCommand == WolfCommand.NONE)
                return true;

            return false;
        }

        public WolfCommand GetCurentCommand()
        {
            return m_CurrentCommand;
        }

        public bool IsCommandCome()
        {
            if (m_CurrentCommand == WolfCommand.COME)
                return true;

            return false;
        }

        private bool IsStayTimerDone()
        {
            if (!m_StayTimerSet)
            {
                m_StayTimerSet = true;
                m_StayEndTime = Time.time + m_MaxTimeToStay;
            }

            if (Time.time < m_StayEndTime)
            {
                return false;
            }

            m_StayTimerSet = false;
            return true;
        }

        #endregion


        // Update is called once per frame
        void Update()
        {

            UpdateFactors();

            // Check for events or player suggestions to switch State

            // Traverse current Behavior Tree
            if (!m_SkipBTUpdate)
            {
                if (!ReferenceEquals(m_CurrentBT, null))
                    m_CurrentBT.ContinueBehaviorTree();
                else
                    Debug.Assert(false, "CompanionAISM: current BT not initialized yet");
            }
        }

        private void UpdateFactors()
        {
            DistToPlayerSq.value = (Player.transform.position - transform.position).sqrMagnitude;

            // Get some data to use
            bool isPlayerStealthed = Player.GetComponent<MoveComponent>().m_Crouching;
            ActionZone curAZ = m_GameControl.CurrentActionZone;

            // Determine if Stealth is correct state to be in
            if (CurrentCommand != WolfCommand.STAY && m_CurrentMainState != WolfMainState.Attack)
            {               
                if (isPlayerStealthed && m_CurrentMainState != WolfMainState.Stealth && curAZ && curAZ.GetNumEnemiesAlive() > 0)
                {
                    SetMainState(WolfMainState.Stealth);
                    return;
                }
            }

            // Continue "GoTo" if not stealthing
            if (CurrentCommand == WolfCommand.GOTO && !isPlayerStealthed && (curAZ == null || curAZ.IsAtFinalStealthPoint(transform.position))){
                m_SkipBTUpdate = true;
                MoveTo(TargetMoveToLocation);
            }


            // See if player is out of action zone and follow if 
            if (m_DidPlayerLeaveZone && curAZ) { 

                if (curAZ.IsAtFinalStealthPoint(transform.position))
                {
                    SetMainState(WolfMainState.Follow);
                    return;
                }
                else if(isPlayerStealthed)
                {
                    m_StealthNav.FindOwnPath = true;
                    SetMainState(WolfMainState.Stealth);
                    m_StealthNav.ExecuteStealthGoToCommand(Player.transform.position);
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
            if(m_CurrentMainState != newState)
                m_PreviousMainState = m_CurrentMainState;

            m_CurrentMainState = newState;

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

                case WolfMainState.Stay:
                    m_CurrentBT = m_StayTree;
                    break;

                default:
                    Debug.Log("Error: CompanionAISM.cs : No Behavior Tree to switch to for desired new state.");
                    break;


            }

            m_CurrentBT.RestartTree();
        }

        //void SetPrevousMainState(WolfMainState newState)
        //{
        //    m_PreviousMainState = newState;
        //}

        #region Command Functions

        /// <summary>
        /// Call this for Commands other than "GoTo"
        /// </summary>
        public void GiveCommand(WolfCommand playerCommand)
        {
            switch (playerCommand)
            {
                case WolfCommand.COME:
                    if (m_GameControl.BondManager.BondStatus < m_ComeBondRequirement)
                    {
                        Debug.Log("Bond is not hight enough to give 'Come' command!");
                        m_AudioContainer.PlaySound((int)GetRandomWhineSound());
                        return;
                    }

                    m_AudioContainer.PlaySound((int)GetRandomBarkSound());

                    if (m_CurrentCommand == WolfCommand.STAY)
                    {
                        //m_Animator.SetBool("Idle", true);
                        m_Animator.Play("SeatToStand");
                    }

                    m_CurrentCommand = WolfCommand.COME;

                    ActionZone curAZ = m_GameControl.GetActionZoneFromPoint(Player.transform.position);
                    //bool comeViaStealth = (curAZ && !curAZ.IsAtFinalStealthPoint(transform.position));
                    //if (comeViaStealth)
                    //{
                    //    SetMainState(WolfMainState.Stealth);
                    //    return;
                    //}

                    if (curAZ)
                    {
                        if (curAZ.IsAtFinalStealthPoint(transform.position))
                        {
                            SetMainState(WolfMainState.Follow);
                        }
                        else
                        {
                            SetMainState(WolfMainState.Stealth);
                            m_StealthNav.ExecuteStealthGoToCommand(Player.transform.position);
                        }
                    }
                    else
                    {
                        SetMainState(WolfMainState.Follow);
                    }

                    break;

                case WolfCommand.STAY:

                    if (m_GameControl.BondManager.BondStatus < m_ComeBondRequirement)
                    {
                        Debug.Log("Bond is not hight enough to give 'Stay' command!");
                        m_AudioContainer.PlaySound((int)GetRandomWhineSound());
                        return;
                    }

                    m_AudioContainer.PlaySound((int)GetRandomBarkSound());

                    m_CurrentCommand = WolfCommand.STAY;
                    SetMainState(WolfMainState.Stay);

                    //m_Animator.SetInteger("IDIdle", 5);
                    //m_Animator.SetBool("Idle", true);
                    m_Animator.Play("IdleSit");
                    break;

            }
        }

        /// <summary>
        /// For a "GoTo" command: Pass in the game object that was hit from the raycast (to check if it was an enemy),
        /// as well as the location that the ray cast hit (to tell the wolf to actually go to that location
        /// </summary>
        public void GiveGoToCommand(GameObject hitObject, Vector3 goToLocation)
        {
            float bondStatus = m_GameControl.BondManager.BondStatus;
            if(bondStatus > m_GoToBondRequirement)
            {
                m_AudioContainer.PlaySound((int)GetRandomBarkSound());

                SetCurrentCommand(WolfCommand.GOTO);

                if (hitObject.GetComponent<EnemyAISM>())
                {
                    // Attack this enemy!
                    Debug.Log("Attack the enemy!");
                    m_EnemyTarget = hitObject;
                    SetMainState(WolfMainState.Attack);
                    SetCurrentCommand(WolfCommand.NONE);

                    //if (m_PreviousMainState != WolfMainState.Attack)
                    m_AudioContainer.PlaySound((int)GetRandomGrowlSound());
                }
                else
                {
                    ActionZone AZPointIsIn = m_GameControl.GetActionZoneFromPoint(goToLocation);
                    if (AZPointIsIn && AZPointIsIn.GetNumEnemiesAlive() > 0)
                    {
                        m_GameControl.CurrentActionZone = AZPointIsIn;
                        
                        SetMainState(WolfMainState.Stealth);
                        m_StealthNav.ExecuteStealthGoToCommand(goToLocation);
                        TargetMoveToLocation = goToLocation;
                        Debug.Log("Executing GoTo in stealth");
                    }
                    else
                    {
                        // Just Go To a point as normal
                        TargetMoveToLocation = goToLocation;
                        MoveTo(goToLocation);
                    }

                }
            }
            else
            {
                Debug.Log("Go To failed! Bond is not high enough to give commands.");
                m_AudioContainer.PlaySound((int)GetRandomWhineSound());
            }
        }

        private void SetCurrentCommand(WolfCommand commandType)
        {
            m_CurrentCommand = commandType;
            //m_CurrentCommandAsInt = new Int((int)commandType);
        }

        #endregion

        #region Movement Functions

        public void MoveTo(Vector3 Location)
        {
            float distToPtSq = (Location - transform.position).sqrMagnitude;
            float minDistSq = 2.0f;
            if (distToPtSq < minDistSq)
            {
                if (m_CurrentCommand == WolfCommand.GOTO && m_CurrentMainState != WolfMainState.Attack)
                {
                    m_CurrentCommand = WolfCommand.NONE;
                    m_SkipBTUpdate = false;
                    m_WolfMoveComp.Stop();
                    SetMainState(WolfMainState.Follow);
                    return;
                }
            }
            /*
            * idealy we'd move this whole calculation into move component,but lets just do this for now
            * and wait to see how much we want the AI to calculate and how much the move comp should calculate
            */
            //Executing following in the move component
            NavMeshPath path = new NavMeshPath();
            WolfNavAgent.CalculatePath(Location, path);
            if (path.corners.Length == 0)
            {
                //Debug.Log("NO PATH FOUND! THIS SHOULD NOT HAPPEN!!");
            }

            if (path.corners.Length != 0)
            {
                if (m_Corners[0] != path.corners[0])
                {
                    m_Corners = path.corners;
                    StartCoroutine(m_WolfMoveComp.Move(Location, m_Corners, WolfNavAgent));
                }
            }
        }

        public void FollowBehind()
        {
            Vector3 toPlayer = Player.transform.position - this.transform.position;
            toPlayer.Normalize();

            TargetMoveToLocation = Player.transform.position - FollowDistance/2 * toPlayer;

            MoveTo(TargetMoveToLocation);
            return;
            /*
             * idealy we'd move this whole calculation into move component,but lets just do this for now
             * and wait to see how much we want the AI to calculate and how much the move comp should calculate
             */
             //Executing following in the move component
            NavMeshPath path = new NavMeshPath();
            WolfNavAgent.CalculatePath(TargetMoveToLocation, path);
            if (path.corners.Length == 0)
            {
                //Debug.Log("NO PATH FOUND! THIS SHOULD NOT HAPPEN!!");
            }
            
            if (path.corners.Length != 0)
            {
                if (m_Corners[0] != path.corners[0])
                {
                    m_Corners = path.corners;
                    StartCoroutine( m_WolfMoveComp.Move(TargetMoveToLocation, m_Corners, WolfNavAgent));
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

        public void EndCommand()
        {
            m_CurrentCommand = WolfCommand.NONE;
        }

        private void PlayRandomIdleAnimation()
        {
            if (Time.time - m_IdleStateStartTime > m_IdleSitTime)
            {
                // Randomly pick between Sit or Sleep
                if ((int)Time.time % 2 == 1)
                {
                    m_Animator.SetInteger("IDIdle", 5);
                    
                }
                else
                {
                    m_Animator.SetInteger("IDIdle", 6);
                }
                m_Animator.SetBool("Idle", true);
                m_IdleStateStartTime = Time.time;
            }
            else {
                int idInt = Random.Range(1, 5);
                m_Animator.SetInteger("IDIdle", idInt);
                m_Animator.SetBool("Idle", true);
            }
        }

        private void PlayIdleAnimation(int animID)
        {
            m_Animator.SetInteger("IDIdle", animID);
            m_Animator.SetBool("Idle", true);
        }

        private bool IsIdleTimerDone()
        {
            if (!m_IdleTimerSet)
            {
                m_IdleTimerSet = true;
                m_IdleCooldownEndTime = Time.time + Random.Range(m_IdleAnimCooldownMin, m_IdleAnimCooldownMax);
            }

            if (Time.time < m_IdleCooldownEndTime)
            {
                return false;
            }

            m_IdleTimerSet = false;
            return true;
        }

        private void StartIdle()
        {
            m_IdleStateStartTime = Time.time;

        }

        public void EndIdleState()
        {
            m_Animator.SetInteger("IDIdle", 0);
            m_Animator.applyRootMotion = true;
            m_Animator.SetBool("Idle", false);
        }

        #endregion

        #region Attack Functions

        private void MoveToEnemy()
        {
            //if(!ReferenceEquals(m_EnemyTarget, null))
            if(m_EnemyTarget != null)
            {
                float distToEnemySq = (m_EnemyTarget.transform.position - transform.position).sqrMagnitude;
                if(distToEnemySq > m_AttackRange * m_AttackRange)
                    MoveTo(m_EnemyTarget.transform.position);
            }
            else
            {
                Debug.Log("Enemy is a null reference!");
                // Reset tree to find a new target
                SetMainState(WolfMainState.Attack);
                m_AudioContainer.PlaySound((int)GetRandomGrowlSound());
            }
        }

        private bool IsAtEnemyTarget()
        {
            if (!m_EnemyTarget)
            {
                DetermineTarget();
                if (!m_EnemyTarget)
                {
                    return false;
                }
            }

            float distToEnemySq = (m_EnemyTarget.transform.position - transform.position).sqrMagnitude;
            bool isAtTarget = (distToEnemySq < m_AttackRange * m_AttackRange);

            //if(!isAtTarget)
             //   m_IsAttacking = false;

            return isAtTarget;
        }

        private void DetermineTarget()
        {
            if (!m_GameControl.CurrentActionZone)
            {
                Debug.Assert(false, "Error! No current Action Zone!");
            }

            int bondStatus = m_GameControl.BondManager.BondStatus;

            if (m_GameControl.CurrentActionZone && m_GameControl.CurrentActionZone.GetNumEnemiesAlive() == 0)
            {
                SetMainState(WolfMainState.Follow);
                return;
            }

            // If Bond is good, select player's targetas my target
            if (bondStatus >= 50)
            {
                if(m_PlayersEnemyTarget && !m_PlayersEnemyTarget.GetComponent<Health>().IsDead)
                    m_EnemyTarget = m_PlayersEnemyTarget;
            }

            // If coming from breaking stealth due to low bond, keep current target
            if (m_EnemyTarget && !m_EnemyTarget.GetComponent<Health>().IsDead && bondStatus < 50)
                return;

            if(!m_EnemyTarget || bondStatus < 50){
                GameObject target = m_GameControl.CurrentActionZone.GetClosestAgrodEnemy(transform.position);
                if (target)
                {
                    m_EnemyTarget = target;
                }
                else
                {
                    // No agrod Enemies, revert to previous state
                    Debug.Log("No agro'd enemies, reverting to previous state");
                    SetMainState(m_PreviousMainState);
                }
            }
        }


        private void AttackMyEnemy()
        {

            {
                m_IsAttacking = true;
                m_WolfMoveComp.Stop();
                GetComponent<Animator>().SetBool("Attack1", true);
                int IDAttack = Random.Range(1, 4);
                GetComponent<Animator>().SetInteger("IDAttack", IDAttack);

                if (Random.Range(1, 3) % 2 == 0)
                    m_AudioContainer.PlaySound((int)GetRandomGrowlSound());
            }
        }

        public void DealDamage()
        {
            if (m_EnemyTarget && m_Health)
            {
                float damage = m_Health.Damage;
                if (m_EnemyTarget.GetComponent<Health>())
                {
                    m_EnemyTarget.GetComponent<Health>().TakeDamage(damage, gameObject);
                }
            }
        }

        public Transform GetEnemyTarget()
        {
            if(m_EnemyTarget)
                return m_EnemyTarget.transform;

            return null;
        }

        private void SetClosestEnemyAsTarget()
        {
            GameObject enemy = m_GameControl.CurrentActionZone.GetClosestEnemyTo(transform.position);
            if (enemy){
                m_EnemyTarget = enemy;
            }

        }

        private bool IsAttackTimerDone()
        {
            if (!m_AttackTimerSet)
            {
                m_AttackTimerSet = true;
                m_AttackCooldownEndTime = Time.time + m_AttackCooldown;
            }

            if (Time.time < m_AttackCooldownEndTime)
            {
                return false;
            }

            m_IsAttacking = false;
            m_AttackTimerSet = false;
            return true;
        }

        public void NotifyPlayerHitTarget(GameObject hitEnemy)
        {
            if (hitEnemy && hitEnemy.GetComponent<EnemyAISM>())
            {
                ActionZone az = hitEnemy.GetComponent<EnemyAISM>().MyActionZone; //m_GameControl.GetActionZoneFromPoint(hitEnemy.transform.position);

                if (az != m_GameControl.CurrentActionZone)
                    m_GameControl.CurrentActionZone = az;

                m_PlayersEnemyTarget = hitEnemy;

                if(m_PreviousMainState != WolfMainState.Attack)
                    m_AudioContainer.PlaySound((int)GetRandomGrowlSound());

                SetMainState(WolfMainState.Attack);
            }
        }

        public void AgroAccalia(GameObject enemyWhoAttacked)
        {
            if(m_CurrentMainState != WolfMainState.Attack)
            {
                m_AudioContainer.PlaySound((int)GetRandomGrowlSound());
                SetMainState(WolfMainState.Attack);
                if (enemyWhoAttacked)
                {
                    m_EnemyTarget = enemyWhoAttacked;
                }
            }
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
                Vector3 EnemyPos = m_EnemyTarget.transform.position;
                Vector3 EnemyDir = m_EnemyTarget.GetComponent<NavMeshAgent>().destination - EnemyPos;
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

        // Tests Bond to see if stealth should be broken and Accalia should be aggressive and attack instead
        private bool ToBreakStealthForAttack()
        {
            // Make the chance based on the bond percentage
            float bondStatus = m_GameControl.BondManager.GetBondStatus();
            if (bondStatus > m_BreakStealthToAttackThreshold)
                return false;

            float percentToCheck = (m_BreakStealthToAttackThreshold - bondStatus) / m_BreakStealthToAttackThreshold;
            float val = Random.Range(0f, 1f);
            Debug.Log("Test if should break stealth: " + val + " < " + percentToCheck);
            if(val < percentToCheck)
            {
                // Enemy has to be close enough to piss her off
                GameObject enemy = m_GameControl.CurrentActionZone.GetClosestEnemyTo(transform.position);

                float distanceThresholdMultiplierSq = 9.0f; // To increase the range that she might attack form
                Debug.Log("EnemyDistance vs threshhold: " + (enemy.transform.position - transform.position).sqrMagnitude + " < " + distanceThresholdMultiplierSq * m_AttackRange * m_AttackRange);
                if (enemy && (enemy.transform.position - transform.position).sqrMagnitude < distanceThresholdMultiplierSq * m_AttackRange * m_AttackRange){
                    m_EnemyTarget = enemy;
                    SetMainState(WolfMainState.Attack);

                    Debug.Log("Breaking stealth to Attack due to low bond!");

                    m_AudioContainer.PlaySound((int)GetRandomGrowlSound());

                    // Flash bond? Pop up window?
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Utillity Functions

        float GetDistToPlayerSq()
        {
            return (Player.transform.position - transform.position).sqrMagnitude;
        }

        public bool IsPlayerOutOfFollowRange()
        {
            return ((Player.transform.position - transform.position).sqrMagnitude > StartToFollowDistance * StartToFollowDistance);
        }

        #endregion

        #region Audio Functions

        private AccaliaSounds GetRandomWhineSound()
        {
            int val = Random.Range(1, 8);
            switch (val) {
                case 1:
                    return AccaliaSounds.WHINE1;
                case 2:
                    return AccaliaSounds.WHINE2;
                case 3:
                    return AccaliaSounds.WHINE3;
                case 4:
                    return AccaliaSounds.WHINE4;
                case 5:
                    return AccaliaSounds.WHINE5;
                case 6:
                    return AccaliaSounds.WHINE6;
                default:
                    return AccaliaSounds.GROWL1;
            }
        }

        private AccaliaSounds GetRandomBarkSound()
        {
            int val = Random.Range(1, 4);
            switch (val)
            {
                case 1:
                    return AccaliaSounds.BARK1;
                case 2:
                    return AccaliaSounds.BARK2;
                default:
                    return AccaliaSounds.BARK3;
            }
        }

        private AccaliaSounds GetRandomGrowlSound()
        {
            int val = Random.Range(1, 4);
            switch (val)
            {
                case 1:
                    return AccaliaSounds.GROWL2;
                case 2:
                    return AccaliaSounds.GROWL3;
                default:
                    return AccaliaSounds.GROWL4;
            }
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

        public void DoEndGame()
        {
            if (GetComponent<Animator>())
            {
                GetComponent<Animator>().Stop();
            }
        }
    }

}
