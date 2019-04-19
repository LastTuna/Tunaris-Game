﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RaceStart : MonoBehaviour {

    public List<AudioClip> RaceStartSounds;// All 3 race start sounds
    public AudioSource AudioSource;// AudioSource playing
    public CanvasGroup gameplayUI;//canvas group for the active UI during race
    public CanvasGroup postRaceUI;//canvas group for the UI that activates post-race
    public bool IsRaceStarted;// Is the race started
    public List<int> currentLap;//current lap.
    public int laps;//amount of laps; changeable in menu?
    public Text currentTime;//text value
    public Text lap1;//the top ticker on UI - alternatively displays best lap
    public Text lap2;//the center ticker on UI - alternatively displays the last completed lap
    public Text lap3;//third ticker on UI / active lap
    public Text countdownText;//countdown visual
    public Text currentLapText;//laps / current lap
    

    public int i;
    public bool raceFinished = false;//flag for when races finished
    public List<TimeSpan> laptimes = new List<TimeSpan>();//list for laptimes
    public TimeSpan CurrentLapTime = new TimeSpan(0, 0, 00, 00, 000);
    public TimeSpan lastLapTime = new TimeSpan(0, 0, 00, 00, 000);//previously completed lap
    public TimeSpan fastestLapTime = new TimeSpan(0, 0, 00, 00, 000);//fastest lap time - updated per lap from the List
    public int countdown;//countdown begins at end race
    public TimeSpan duration = new TimeSpan(0, 0, 00, 00, 000);//creates new timespan,
    //used to tally time from beginning of race

    public IEnumerator CountDown() {
        FindObjectOfType<CourseController>().OnRaceStart();
        for (int i = 0; i < RaceStartSounds.Count - 1; i++) {
            AudioSource.PlayOneShot(RaceStartSounds[i]);
            yield return new WaitForSeconds(1);
        }
        AudioSource.PlayOneShot(RaceStartSounds[RaceStartSounds.Count - 1]);
        IsRaceStarted = true;
    }

    public void StartRace(int position)
    {
        StartCoroutine(CountDown());

        GameObject gridSpot = GameObject.Find("Pos_" + position);
        FindObjectOfType<CarBehaviour>().gameObject.transform.position = gridSpot.transform.position;
        FindObjectOfType<CarBehaviour>().gameObject.transform.rotation = gridSpot.transform.rotation;
        //foreach
    }

    void Update() {
        if (IsRaceStarted) {
            LaptimeTicker();
            currentLapText.text = ("LAP: " + (currentLap[0] + 1) + "/" + (laps));

            if (laps <= currentLap[0] || raceFinished)
            {
                //change this, to check EVERY PLAYERS VALUE OF RACE COMPLETE. IF RACE IS COMPLETE FOR SOMEONE
                //THEN TRIGGER END RACE
                IsRaceStarted = false;
                StartCoroutine(EndRace());
            }
        }
    }

    public void LapCompleted(string player)
    {
        currentLap[0]++;
        laptimes.Add(CurrentLapTime);//tally current lap time to List
        CurrentLapTime = CurrentLapTime.Subtract(CurrentLapTime - TimeSpan.FromMilliseconds(1));//reset current lap timer
        
        //tally on screen values
        if (currentLap[0] > 2)
        {
            lastLapTime = laptimes[currentLap[0] - 1];
            fastestLapTime = laptimes.Min();
            //lap 1 = best lap
            //lap 2 = last completed lap time
            //lap 3 = current lap
            lap1.text = string.Format("{0:00}:{1:00}:{2:000}", fastestLapTime.Minutes, fastestLapTime.Seconds, fastestLapTime.Milliseconds);
            lap2.text = string.Format("{0:00}:{1:00}:{2:000}", lastLapTime.Minutes, lastLapTime.Seconds, lastLapTime.Milliseconds);
        }
        else if (currentLap[0] > 1)
        {
            //lap 1 = first lap
            //lap 2 = second lap
            //lap 3 = current lap
            lap1.text = string.Format("{0:00}:{1:00}:{2:000}", laptimes[currentLap[0] - 2].Minutes, laptimes[currentLap[0] - 2].Seconds, laptimes[currentLap[0] - 2].Milliseconds);
            lap2.text = string.Format("{0:00}:{1:00}:{2:000}", laptimes[currentLap[0] - 1].Minutes, laptimes[currentLap[0] - 1].Seconds, laptimes[currentLap[0] - 1].Milliseconds);
        }
        else if (currentLap[0] > 0)
        {
            lap2.text = string.Format("{0:00}:{1:00}:{2:000}", laptimes[currentLap[0] - 1].Minutes, laptimes[currentLap[0] - 1].Seconds, laptimes[currentLap[0] - 1].Milliseconds);
        }
    }

    void LaptimeTicker() {
        //counter: TOTAL TIME
        duration = duration.Add(TimeSpan.FromMilliseconds(Time.deltaTime * 1000));
        currentTime.text = string.Format("{0:00}:{1:00}:{2:000}", duration.Minutes, duration.Seconds, duration.Milliseconds);

        //end TOTAL TIME
        //current lap time
        CurrentLapTime = CurrentLapTime.Add(TimeSpan.FromMilliseconds(Time.deltaTime * 1000));
        lap3.text = string.Format("{0:00}:{1:00}:{2:000}", CurrentLapTime.Minutes, CurrentLapTime.Seconds, CurrentLapTime.Milliseconds);
    }

    public IEnumerator EndRace() {
        raceFinished = true;
        gameplayUI.alpha = 0f;
        postRaceUI.alpha = 1f;
        i = 30;
        while (i > 0)
        {
            //countdown down 30sec, after first placer crosses line. after that trigger post race
            countdownText.text = i.ToString();
            i--;
            yield return new WaitForSecondsRealtime(1);
        }

        yield return new WaitForSecondsRealtime(4);
        Time.timeScale = 0.0F;


        //add trigger to camera script - quit following the car with a moderate damping effect, and move upwards
        //ill make a refrence or something of how it should go

        //add trigger to car brake, so when you cross finish line it would brake (carBehaviour.cs)
        //make the post-race UI and expose it on screen,
        //make the button to restart/exit
        //comes in english
        FindObjectOfType<PostRace>().enabled = true;
        DataController dataController = FindObjectOfType<DataController>();
        dataController.Dirtiness[FindObjectOfType<CarBehaviour>().carIndex] = FindObjectOfType<CarBehaviour>().dirtiness;

        yield return new WaitForSecondsRealtime(4);
        Time.timeScale = 1.0F;

        FindObjectOfType<CourseController>().Cleanup();
        SceneManager.LoadScene(3, LoadSceneMode.Single);
        GameObject.Find("DataController").GetComponent<PostRace>().checkum = true;

    }
}