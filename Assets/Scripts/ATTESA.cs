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
        WheelHit wheelhitL;
        WheelHit wheelhitR;
        if (wheelRL.GetGroundHit(out wheelhitL) && wheelRR.GetGroundHit(out wheelhitR))
        {
            if(wheelhitL.sidewaysSlip > 0.05f || wheelhitR.sidewaysSlip > 0.05f)
            {
                wheelRL.steerAngle = SlipAvg(wheelhitL, wheelhitR) * 30 + ( - wheelFL.steerAngle / 6);
                wheelRR.steerAngle = SlipAvg(wheelhitL, wheelhitR) * 30 + ( - wheelFR.steerAngle / 6);
            }
            else
            {
                wheelRL.steerAngle = -wheelFL.steerAngle / 6;
                wheelRR.steerAngle = -wheelFR.steerAngle / 6;
            }
        }
    }

    float SlipAvg(WheelHit wheelL, WheelHit wheelR)
    {
        return (wheelL.sidewaysSlip + wheelR.sidewaysSlip) / 2; ;
    }

}
