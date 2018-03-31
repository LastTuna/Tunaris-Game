using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CheckpointFlag : MonoBehaviour {

    public bool checkum = false;
    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerStay(Collider other)
    {
        checkum = true;
    }


}
