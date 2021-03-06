﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATTESA : MonoBehaviour {
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;
    public CarBehaviour carBehaviour;

    // Update is called once per frame
    void FixedUpdate() {
        WheelHit wheelhitL;
        WheelHit wheelhitR;
        //detect slip for rear wheels and steer accordingly
        if (wheelRL.isGrounded || wheelRR.isGrounded)
        {
            wheelRL.GetGroundHit(out wheelhitL);
            wheelRR.GetGroundHit(out wheelhitR);
            if (Mathf.Abs(SlipAvg(wheelhitL, wheelhitR)) > 0.05f && wheelRL.rpm + wheelRR.rpm > 300)
            {
                wheelRL.steerAngle = SlipAvg(wheelhitL, wheelhitR) * 30 + (-wheelFL.steerAngle / 6);
                wheelRR.steerAngle = SlipAvg(wheelhitL, wheelhitR) * 30 + (-wheelFR.steerAngle / 6);
            }
            else
            {
                wheelRL.steerAngle = -wheelFL.steerAngle / 6;
                wheelRR.steerAngle = -wheelFR.steerAngle / 6;
            }
            ActiveDifferential(wheelhitL, wheelhitR);
        } else {
            //wheelRL.steerAngle = 0;
            //wheelRR.steerAngle = 0;
        }
    }

    float SlipAvg(WheelHit wheelL, WheelHit wheelR) {
        return (wheelL.sidewaysSlip + wheelR.sidewaysSlip) / 2;
    }

    void ActiveDifferential(WheelHit wheelL, WheelHit wheelR) {
        if (wheelL.forwardSlip + wheelR.forwardSlip > 0.5f) {
            carBehaviour.specs.frontWheelDriveBias = 0.5f;
        } else {
            carBehaviour.specs.frontWheelDriveBias = Mathf.Abs(wheelL.forwardSlip + wheelR.forwardSlip);
        }
    }

}