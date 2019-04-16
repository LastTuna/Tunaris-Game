using System;
using UnityEngine;

public class CheckpointReader : MonoBehaviour {

    public int nextCheck = 0;
    RaceStart raceManager;
    void Start()
    {
        raceManager = FindObjectOfType<RaceStart>();
    }

    void OnTriggerEnter(Collider checkpoint)
    {
        if(checkpoint.tag == "checkpoint")
        {
            if(int.Parse(checkpoint.name) == nextCheck + 1)
            {
                nextCheck++;
            }
            if (int.Parse(checkpoint.name) == 0 && nextCheck > 0)
            {
                nextCheck = 0;
                raceManager.LapCompleted();
            }
        }
    }
}
