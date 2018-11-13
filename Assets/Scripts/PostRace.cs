using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class PostRace : MonoBehaviour
{

    public bool checkum;
    public int laps;
    public int count;
    public GameObject textInstance;
    public List<GameObject> textInstances = new List<GameObject>();
    public Vector3 spawnPos;
    public List<TimeSpan> lapTally = new List<TimeSpan>();
    public Canvas mainCanvas;

    // Use this for initialization
    void Start()
    {
        checkum = true;
        lapTally = GameObject.Find("CourseController").GetComponent<RaceStart>().laptimes;
        laps = lapTally.Count - 1;
    }

    // Update is called once per frame
    void Update()
    {


        if (GameObject.Find("0") && checkum)
        {
            mainCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            StartCoroutine(TallyLaps());
        }
    }

    IEnumerator TallyLaps()
    {
        checkum = false;
        float raise = 0;//raise next element by x amount
        GameObject despacito;
        foreach (TimeSpan e in lapTally)
        {
            despacito = null;//clears object reference
            despacito = Instantiate(textInstance, new Vector3(spawnPos.x, spawnPos.y + raise, 0), new Quaternion(0, 0, 0, 0), mainCanvas.transform);
            //create new laptime object/element
            despacito.GetComponent<Text>().text = string.Format("{0:00}:{1:00}:{2:000}", lapTally[count].Minutes, lapTally[count].Seconds, lapTally[count].Milliseconds);
            if (e.Equals(lapTally.Min()))
            {//bestest lap time
                despacito.GetComponent<Text>().text += " BEST";
                despacito.GetComponent<Text>().color = new Color(200,0,0);
            }
            textInstances.Add(despacito);//add all elements to list (will use later)
            raise += 30;//raise by x amount
            count++;//tally/loop
            yield return new WaitForSeconds(0.1f);
            Debug.Log(count);
        }
        // To do:
        // patch so the text is spawned at correct place

        //fcr when u get those done^ u can leave rest to me

        // make fucking unity stop sperging about null exceptions despite all laps being shown lol - DONE!
        // Make a dynamic lap tally (this WILL crash if it's above 10 laps!!!) - DONE!
        //achieve by using instantiate() and make a prefab with text element - DONE!
        // Add "Best Lap" option @ - DONE


        //laptime[count] = GameObject.Find(Convert.ToString(count)).GetComponent<Text>();
        //laptime[count].text = string.Format("{0:00}:{1:00}:{2:000}", lapTally[count].Minutes, lapTally[count].Seconds, lapTally[count].Milliseconds); 

        yield return new WaitForSecondsRealtime(4);
        SceneManager.LoadScene(0, LoadSceneMode.Single);
        //disabled scene load for easier debugging


    }
}