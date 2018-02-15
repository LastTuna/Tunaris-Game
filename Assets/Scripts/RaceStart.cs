using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RaceStart : MonoBehaviour {
    // All 3 race start sounds
    public List<AudioClip> RaceStartSounds;
    // AudioSource playing
    public AudioSource AudioSource;
    // Is the race started
    public bool IsRaceStarted;

    public Text currentTime;
    public TimeSpan duration = new TimeSpan(0, 0, 00, 00, 000);

    public IEnumerator CountDown() {
        for(int i = 0; i < RaceStartSounds.Count - 1; i++) {
            AudioSource.PlayOneShot(RaceStartSounds[i]);
            yield return new WaitForSeconds(1);
        }
        AudioSource.PlayOneShot(RaceStartSounds[RaceStartSounds.Count - 1]);
        IsRaceStarted = true;
    }

    void Start() {
        StartCoroutine(CountDown());
    }

    void Update() {
        if (IsRaceStarted) {
            currentTime.text = string.Format("{0:00}:{1:00}:{2:000}", duration.Minutes, duration.Seconds, duration.Milliseconds);
            duration = duration.Add(TimeSpan.FromMilliseconds(Time.deltaTime * 1000));
        }
    }
}