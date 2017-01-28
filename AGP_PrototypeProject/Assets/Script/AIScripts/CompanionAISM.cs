using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MalbersAnimations;


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

        public delegate void TriggerActionComplete(bool isComplete);

        public event TriggerActionComplete OnActionComplete;

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

        #endregion


        [SerializeField]
        public GameObject Player;
        private Vector3 playerLoc;

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
        Condition cond2;
        Action printMe;

        #endregion


        // Use this for initialization
        void Start()
        {

            m_CurrentMainState =  WolfMainState.Idle;
            m_PreviousMainState = WolfMainState.Attack;
            WolfNavAgent = GetComponentInParent<NavMeshAgent>();
            //testConditionValue = new Int(0);
            //Int testRight = new Int(1);


            //cond2 = new Condition(testConditionValue, ConditionComparison.Equal, testRight);

            //printMe = new Action(new VoidTypeDelegate(TestActionPrintFunc));


            playerLoc = Player.transform.position;
            DistToPlayerSq = new Float((playerLoc - transform.position).sqrMagnitude);

            InitializeStateBehaviorTrees();
        }

        private void InitializeStateBehaviorTrees()
        {
            CreateIdleBT();
            CreateFollowBT();

            // Start in Idle state
            m_CurrentBT = m_IdleTree;
        }

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
            Condition sfCond1 = new Condition(DistToPlayerSq, ConditionComparison.Less, new Float(FollowDistance * FollowDistance));
            Action switchToIdleAction = new Action(SetMainState, WolfMainState.Idle);

            stopFollowingNode.AddCondition(sfCond1);
            stopFollowingNode.AddAction(switchToIdleAction);

            // Add new Node to parent
            m_FollowTree.AddDecisionNodeTo(rootNode, stopFollowingNode);

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

        IEnumerator waitFiveSeconds()
        {
            yield return new WaitForSeconds(5);

            testConditionValue.value = 1;
        }

        // Update is called once per frame
        void Update()
        {

            UpdateFactors();

            // Check for events or player suggestions to switch State

            // Traverse current Behavior Tree
            m_CurrentBT.ContinueBehaviorTree();


            // Determine what the current state should be
            //if (!cond2.IsMet())
            // UpdateStateMachine();

        }

        private void UpdateFactors()
        {
            DistToPlayerSq.value = (Player.transform.position - transform.position).sqrMagnitude;
        }


        public override void UpdateStateMachine()
        {
            // Update Factors?

            // Execute current state
            switch (CurrentMainState)
            {
                case WolfMainState.Idle:
                    ExecuteIdle();
                    break;

                case WolfMainState.Follow:
                    ExecuteFollow();
                    break;

                case WolfMainState.Stealth:
                    ExecuteStealth();
                    break;

                case WolfMainState.Alert:
                    ExecuteAlert();
                    break;

                case WolfMainState.Attack:
                    ExecuteAttack();
                    break;
            }
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
        }

        public void FollowBehind()
        {
            Vector3 toPlayer = Player.transform.position - this.transform.position;
            toPlayer.Normalize();

            TargetMoveToLocation = Player.transform.position - FollowDistance * toPlayer;
            MoveTo(TargetMoveToLocation);
        }
        #endregion

        #region Idle Functions

        public void DoNothing()
        {
            OnActionComplete(true);
        }

        #endregion

        #region State Execution Functions

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
                    WolfNavAgent.SetDestination(TargetMoveToLocation);
                    break;

                case WolfFollowSubState.FollowAlongside:



                    break;

            }

        }



        void ExecuteAlert()
        {

        }

        void ExecuteStealth()
        {

        }

        void ExecuteAttack()
        {

        }

        #endregion

        void TempPlayerMovement()
        {
            float walkSpeed = 10.0f;

            if (Input.GetKeyDown(KeyCode.W))
            {
                //Player.transform.Translate()   
            }
            if (Input.GetKeyDown(KeyCode.A))
            {

            }
            if (Input.GetKeyDown(KeyCode.S))
            {

            }
            if (Input.GetKeyDown(KeyCode.D))
            {

            }
        }


        public void TestActionPrintFunc()
        {
            Debug.Log("Action delegate worked!!!");
        }
    }

}