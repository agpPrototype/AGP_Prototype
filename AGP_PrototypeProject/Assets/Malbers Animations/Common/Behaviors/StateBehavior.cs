using UnityEngine;
using AI;


//Send Messages for the Animal Script

public class StateBehavior : StateMachineBehaviour
{
    [Header("Send Messages |Enter:true|Exit:false|")]
    public string State;
    private CompanionAISM m_CompanionAISM;
    private Transform m_enemy;
    private Transform m_owner;
    private float m_TurningRate = 30f;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_CompanionAISM = animator.transform.GetComponent<CompanionAISM>();
        if (m_CompanionAISM)
        {
            m_enemy = m_CompanionAISM.GetEnemyTarget();
        }
        m_owner = animator.transform.gameObject.transform;
        animator.SendMessage(State, true, SendMessageOptions.DontRequireReceiver);
        animator.applyRootMotion = false;
    }
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SendMessage(State, false, SendMessageOptions.DontRequireReceiver);
        int IDAttack = Random.Range(1, 4);
        animator.SetInteger("IDAttack", IDAttack);
        animator.applyRootMotion = true;
        animator.SetBool("Attack1", false);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (m_enemy)
        {
            Vector3 dest = m_enemy.transform.position;
            dest.y = m_owner.position.y;
            Vector3 dir = dest - m_owner.position;
            float step = m_TurningRate * Time.deltaTime;
            Vector3 newDir = Vector3.RotateTowards(m_owner.forward, dir, step, 0.0F);
            m_owner.rotation = Quaternion.LookRotation(newDir);
        }
    }
}
