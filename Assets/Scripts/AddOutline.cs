using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddOutline : StateMachineBehaviour {

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (animator.gameObject.transform.childCount > 0 && animator.gameObject.transform.GetChild(0).GetType() == typeof(RectTransform)) {
            animator.gameObject.transform.GetChild(0).gameObject.AddComponent(typeof(Outline));
            animator.gameObject.transform.GetChild(0).gameObject.GetComponent<Outline>().effectColor = Color.red;
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (animator.gameObject.transform.childCount > 0 && animator.gameObject.transform.GetChild(0).GetType() == typeof(RectTransform)) {
            Destroy(animator.gameObject.transform.GetChild(0).gameObject.GetComponent<Outline>());
        }
    }

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}
