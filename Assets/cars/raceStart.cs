using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceStart : MonoBehaviour {
    // All 3 race start sounds
    public List<AudioClip> RaceStartSounds;
    // AudioSource playing
    public AudioSource AudioSource;

    public IEnumerator CountDown() {
        foreach (AudioClip clip in RaceStartSounds) {
            AudioSource.PlayOneShot(clip);
            yield return new WaitForSeconds(1);
        }
    }
}