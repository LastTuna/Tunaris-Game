using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PostRace : MonoBehaviour {
    
    public bool checkum;
    public int laps;
    public int count;
    public GameObject textInstance;
    public static GameObject[] textInstances;
    public Vector3 spawnPos;
    public List<TimeSpan> lapTally = new List<TimeSpan>();

    // Use this for initialization
    void Start ()
    {
        checkum = true;
        lapTally = GameObject.Find("CourseController").GetComponent<RaceStart>().laptimes;
        laps = lapTally.Count - 1;
    }
	
	// Update is called once per frame
	void Update () {


        if (GameObject.Find("0") && checkum)
        {

            StartCoroutine(TallyLaps());
        }
	}

    IEnumerator TallyLaps()
    {
        checkum = false;
        Debug.Log(count);
        float raise = 0;
        int i = 0;
            foreach(TimeSpan e in lapTally)
            {
            textInstances[i] = Instantiate(textInstance, new Vector3(spawnPos.x, spawnPos.y + raise, 0), new Quaternion(0, 0, 0, 0));
            textInstances[i].GetComponent<Text>().text = e.ToString();
            raise += 30;
            yield return new WaitForSeconds(0.1f);
            }
            // To do:
            // Make a dynamic lap tally (this WILL crash if it's above 10 laps!!!)
            //achieve by using instantiate() and make a prefab with text element
            // Add "Best Lap" option @ 
            // make fucking unity stop sperging about null exceptions despite all laps being shown lol

            //laptime[count] = GameObject.Find(Convert.ToString(count)).GetComponent<Text>();
            //laptime[count].text = string.Format("{0:00}:{1:00}:{2:000}", lapTally[count].Minutes, lapTally[count].Seconds, lapTally[count].Milliseconds); 
        
        yield return new WaitForSecondsRealtime(4);
        SceneManager.LoadScene(0, LoadSceneMode.Single);

        

    }
}