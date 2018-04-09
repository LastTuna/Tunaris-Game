using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PostRace : MonoBehaviour {

    public int i = 1;
    public int laps;
    public Text laptime1;
    public Text laptime2;
    public Text laptime3;
    public Text laptime4;
    public Text laptime5;
    public Text laptime6;
    public Text laptime7;
    public Text laptime8;
    public Text laptime9;
    public Text laptime10;


    public List<TimeSpan> lapTally = new List<TimeSpan>();
    // Use this for initialization
    void Start ()
    {
        lapTally = GameObject.Find("CourseController").GetComponent<RaceStart>().laptimes;
        laps = lapTally.Count - 1;
    }
	
	// Update is called once per frame
	void Update () {


        if (i > 0)
        {
            StartCoroutine(TallyLaps());
            i--;
        }
	}

    IEnumerator TallyLaps()
    {
        yield return new WaitForSecondsRealtime(4);
        laptime1 = GameObject.Find("lap1").GetComponent<Text>();
        laptime2 = GameObject.Find("lap2").GetComponent<Text>();
        laptime3 = GameObject.Find("lap3").GetComponent<Text>();
        laptime4 = GameObject.Find("lap4").GetComponent<Text>();
        laptime5 = GameObject.Find("lap5").GetComponent<Text>();
        laptime6 = GameObject.Find("lap6").GetComponent<Text>();
        laptime7 = GameObject.Find("lap7").GetComponent<Text>();
        laptime8 = GameObject.Find("lap8").GetComponent<Text>();
        laptime9 = GameObject.Find("lap9").GetComponent<Text>();
        laptime10 = GameObject.Find("lap10").GetComponent<Text>();
        laptime1.text = string.Format("{0:00}:{1:00}:{2:000}", lapTally[0].Minutes, lapTally[0].Seconds, lapTally[0].Milliseconds);
        laptime2.text = string.Format("{0:00}:{1:00}:{2:000}", lapTally[1].Minutes, lapTally[1].Seconds, lapTally[1].Milliseconds);
        laptime3.text = string.Format("{0:00}:{1:00}:{2:000}", lapTally[2].Minutes, lapTally[2].Seconds, lapTally[2].Milliseconds);
        laptime4.text = string.Format("{0:00}:{1:00}:{2:000}", lapTally.Min().Minutes, lapTally.Min().Seconds, lapTally.Min().Milliseconds);
        laptime5.text = "null";
        laptime6.text = "null";
        laptime7.text = "null";
        laptime8.text = "null";
        laptime9.text = "null";
        laptime10.text = "error#364";
        //laptime1.text = string.Format("{0:00}:{1:00}:{2:000}", laptally[0].Minutes, laptimes[0].Seconds, laptimes[0].Milliseconds);
        yield return new WaitForSecondsRealtime(4);
        SceneManager.LoadScene(0, LoadSceneMode.Single);

    }
}
