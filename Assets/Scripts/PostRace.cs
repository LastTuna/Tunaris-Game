using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PostRace : MonoBehaviour {
    
    public List<TimeSpan> lapTally = new List<TimeSpan>();
    // Use this for initialization
    void Start () {
        lapTally = GameObject.Find("CourseController").GetComponent<RaceStart>().laptimes;
    }
	
	// Update is called once per frame
	void Update () {

		
	}
}
