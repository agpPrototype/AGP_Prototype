using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI;

public class EnemyAttackSMB : StateMachineBehaviour
{
    private EnemyAISM m_EnemyAISM;
    private Transform m_Target;
    private Transform m_owner;
    private float m_TurningRate = 30f;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_EnemyAISM = animator.transform.GetComponent<EnemyAISM>();
        m_owner = animator.transform.gameObject.transform;
        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(layerIndex);
    }
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
}
