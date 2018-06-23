using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CheckpointFlag : MonoBehaviour {

    public bool checkum = false;
    public AudioClip sound;
    public AudioSource soundOutput;

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerStay(Collider other)
    {
        if (!checkum)
        {
            soundOutput.clip = sound;
            soundOutput.Play();
            checkum = true;
        }

    }


}
