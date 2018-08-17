using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarWash : MonoBehaviour {

    public DataController dataController;

	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {


	}
    public void WashMe ()
    {
        dataController.Cash += -10;
        //this cars dirt index, noll it.

    }

}
