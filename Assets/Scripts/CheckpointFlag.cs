using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CheckpointFlag : MonoBehaviour {

    public bool checkum = false;
    public Text ass;
    // Use this for initialization
    void Start () {
        ass.text = "england";
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerStay(Collider other)
    {
        checkum = true;
        ass.text = "true";
    }


}
