using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class raceStart : MonoBehaviour {
    public AudioSource beep;
    public AudioSource launch;
    // Use this for initialization
    void Start () {
       StartCoroutine(CountDown());

	}

    IEnumerator CountDown ()
    {
        yield return new WaitForSeconds(1.0f);
        beep.Play();
        yield return new WaitForSeconds(1.0f);
        beep.Play();
        yield return new WaitForSeconds(1.0f);
        launch.Play();
    }

	// Update is called once per frame
	void Update () {
		
	}
}
