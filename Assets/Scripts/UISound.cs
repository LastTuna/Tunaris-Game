using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISound : StateMachineBehaviour {
    public AudioClip clip;

    void OnStateEnter() {
        // Get the audio source attached to the main camera
        AudioSource audioSource = GameObject.Find("Main Camera").GetComponent<AudioSource>();
        audioSource.PlayOneShot(clip);
    }
}
