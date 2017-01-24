using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CompanionAISM : AIStateMachine {

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

    [SerializeField]
    public GameObject Player;


#region Temp Variables For Testing (To go in another file later)

    private NavMeshAgent WolfNavAgent;
    private Vector3 TargetMoveToLocation;

    [SerializeField]
    private float FollowDistance;

    [SerializeField]
    private Int testConditionValue;

    [SerializeField]
    Condition cond2;
    Action printMe;

#endregion


    // Use this for initialization
    void Start () {

        SetMainState(WolfMainState.Follow);
        SetPrevousMainState(WolfMainState.Attack);
        WolfNavAgent = GetComponentInParent<NavMeshAgent>();

        testConditionValue = new Int(0);
        Int testRight = new Int(1);


        cond2 = new Condition(testConditionValue, ConditionComparison.Equal, testRight);

        printMe = new Action(new VoidTypeDelegate(TestActionPrintFunc));

        StartCoroutine(waitFiveSeconds());

	}
	
    IEnumerator waitFiveSeconds()
    {
        yield return new WaitForSeconds(5);

        testConditionValue.value = 1;
    }

	// Update is called once per frame
	void Update () {

        // Check for events or player suggestions

        // Determine what the current state should be
        if (!cond2.IsMet())
            UpdateStateMachine();
        else
            printMe.PerformAction();

            //TempPlayerMovement();
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
        m_CurrentMainState = newState;
    }

    void SetPrevousMainState(WolfMainState newState)
    {
        m_PreviousMainState = newState;
    }


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
            if(CurrentFollowState == WolfFollowSubState.FollowAlongside)
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

        Vector3 playerLocation = Player.transform.position;
        
        switch (CurrentFollowState)
        {


            case WolfFollowSubState.FollowBehind: // maybe make seperate functions for different sub states
                Vector3 toPlayer = playerLocation - this.transform.position;
                toPlayer.Normalize();

                TargetMoveToLocation = playerLocation - FollowDistance * toPlayer;
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
