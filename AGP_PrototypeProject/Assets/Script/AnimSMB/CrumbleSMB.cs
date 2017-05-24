using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrumbleSMB : StateMachineBehaviour
{
    float maxTime = 12f;
    float compiledTime = 0.0f;
    Transform clone;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        clone = animator.gameObject.transform;
    }
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        compiledTime += Time.deltaTime;
        if (compiledTime >= maxTime)
        {
            //imator.SetBool("End", true);
            animator.StopPlayback();
        }
        else
        {
            clone.position = new Vector3(clone.position.x, clone.position.y - 1.2f * Time.deltaTime, clone.position.z);
        }
    }
}
