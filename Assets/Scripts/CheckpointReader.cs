using UnityEngine;

public class CheckpointReader : MonoBehaviour {

    public int nextCheck = 0;
    public string username;
    void Start()
    {

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
                FindObjectOfType<RaceStart>().LapCompleted(username);//add to detect username
            }
        }
    }
}
