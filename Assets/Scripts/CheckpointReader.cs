using UnityEngine;

public class CheckpointReader : MonoBehaviour {

    int nextCheck = 0;
    RaceStart raceManager;
    void Start()
    {
        raceManager = FindObjectOfType<RaceStart>();
    }

    private void OnTriggerStay(Collider checkpoint)
    {
        if(checkpoint.tag == "checkpoint")
        {
            if(int.Parse(checkpoint.name) + 1 == nextCheck)
            {
                nextCheck++;
            }
            if (int.Parse(checkpoint.name) == 0)
            {
                nextCheck = 0;
                raceManager.LapCompleted();
            }
        }
    }
}
