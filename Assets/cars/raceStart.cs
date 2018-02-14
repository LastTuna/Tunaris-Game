using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class raceStart : MonoBehaviour {
    public AudioSource beep;
    public AudioSource launch;
    public Text currentTime;
    public TimeSpan duration = new TimeSpan(0, 0, 00, 00, 000);

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
        
        currentTime.text = string.Format("{0:00}:{1:00}:{2:000}", duration.Minutes, duration.Seconds, duration.Milliseconds);
        duration = duration.Add(TimeSpan.FromMilliseconds(Time.deltaTime*1000));


    }


}
