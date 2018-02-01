using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISound : StateMachineBehaviour {
    public AudioClip clip;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        // Get the audio source attached to the main camera
        if (animator.GetBool("PlaySound")) {
            AudioSource audioSource = GameObject.Find("Main Camera").GetComponent<AudioSource>();
            audioSource.PlayOneShot(clip);
        }
    }
}
