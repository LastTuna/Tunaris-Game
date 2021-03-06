﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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
        
        // Set transform
        spinner.transform.localScale = new Vector3(120, 120, 120);
        spinner.transform.localPosition = new Vector3(220, -50, -200);

        // Add rotation script
        spinner.AddComponent<Spinner>();

        // Keep track for the spinner
        spinners.Add(spinner);
    }
}