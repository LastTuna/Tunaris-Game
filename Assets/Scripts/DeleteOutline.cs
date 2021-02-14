using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeleteOutline : StateMachineBehaviour {
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (animator.gameObject.transform.childCount > 0 && animator.gameObject.transform.GetChild(0).GetType() == typeof(RectTransform)) {
            Outline[] outlines = animator.gameObject.transform.GetComponentsInChildren<Outline>();
            foreach (Outline o in outlines) {
                Destroy(o);
            }
        }
    }
}
