using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowCarSpinner : StateMachineBehaviour {
    public GameObject spinner;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        Debug.Log("OnStateEnter");
        // Instantiate prefab
        ButtonProperties data = animator.gameObject.GetComponent<ButtonProperties>();
        spinner = Instantiate(data.carPrefab, data.parent.transform);

        // Disable main rigidbody
        Destroy(spinner.GetComponent<Rigidbody>());
        // Disable wheel colliders or unity spergs in the log
        foreach (WheelCollider wc in spinner.GetComponentsInChildren<WheelCollider>()) {
            Destroy(wc);
        }
        // Disable car driving scripts
        foreach (Behaviour c in spinner.GetComponents(typeof(Behaviour))) {
            Destroy(c);
        }

        // Set transform
        spinner.transform.localScale = new Vector3(120, 120, 120);
        spinner.transform.localPosition = new Vector3(220, -50, -200);

        // Add rotation script
        spinner.AddComponent<Spinner>();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        Debug.Log("OnStateExit");
        Destroy(spinner);
    }
}
