using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATTESA : MonoBehaviour {

    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;

    // Use this for initialization
    void Start () {
		
	}

    // Update is called once per frame
    void Update()
    {
        WheelHit wheelhit;
        if (wheelRL.GetGroundHit(out wheelhit))
        {
            if(wheelhit.sidewaysSlip > 0.05f)
            {
                wheelRL.steerAngle = wheelhit.sidewaysSlip * 30 + ( - wheelFL.steerAngle / 6);
                wheelRR.steerAngle = wheelhit.sidewaysSlip * 30 + ( - wheelFR.steerAngle / 6);
                Debug.Log(wheelhit.sidewaysSlip);
            }
            else
            {
                wheelRL.steerAngle = -wheelFL.steerAngle / 6;
                wheelRR.steerAngle = -wheelFR.steerAngle / 6;
            }
        }
    }

}
