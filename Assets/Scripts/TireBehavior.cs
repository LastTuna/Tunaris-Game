using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TireBehavior : MonoBehaviour {

    public WheelCollider tyre;
    public float tyreType;
    public float TreadHealth = 100;
    public float diameter;
    public bool burst = false;//feature to be added? tire wear causes more prone to bursting
    
    // Use this for initialization
    void Start () {
        tyreType = GameObject.FindObjectOfType<DataController>().TireBias;
        diameter = tyre.radius;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        TyreWear();




    }
    public void TyreWear()
    {
        WheelHit CorrespondingGroundHit;
        tyre.GetGroundHit(out CorrespondingGroundHit);

        if (Mathf.Abs(CorrespondingGroundHit.sidewaysSlip) > .6)
        {
            TreadHealth = TreadHealth - CorrespondingGroundHit.sidewaysSlip / 10;
        }
    }

}
