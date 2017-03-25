using UnityEngine;
using AI;


public class WolfIdleSMB : StateMachineBehaviour
{
    [Header("Send Messages |Enter:true|Exit:false|")]
    public string State;
    private CompanionAISM m_CompanionAISM;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_CompanionAISM = animator.transform.GetComponent<CompanionAISM>();
        if (m_CompanionAISM)
        {

        }

    }
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetInteger("IDIdle", 0);
        animator.applyRootMotion = true;
        animator.SetBool("Idle", false);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // If we told Accalia to stay, keep her in position
        if (m_CompanionAISM.GetCurentCommand() == AI.WolfCommand.STAY && State == "IdleSit")
            return;

        if (State == "IdleSit" || State == "IdleSleep")
        {
            bool isPlayerOutOfRange = animator.transform.GetComponent<CompanionAISM>().IsPlayerOutOfFollowRange();
            if (isPlayerOutOfRange)
            {
                if (State == "IdleSit")
                    animator.Play("SeatToStand");
                else if (State == "IdleSleep")
                    animator.Play("SleepToSeat");
            }
        }
        
    }
    
}
