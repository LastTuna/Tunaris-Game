﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowCarSpinner : StateMachineBehaviour {
    public static List<GameObject> spinners = new List<GameObject>();

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        // Destroys all the spinners
        foreach (GameObject car in spinners) {
            Destroy(car);
        }

        // Instantiate prefab
        ButtonProperties data = animator.gameObject.GetComponent<ButtonProperties>();
        GameObject spinner = Instantiate(data.carPrefab, data.parent.transform);

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

        // Keep track for the spinner
        spinners.Add(spinner);
    }
}