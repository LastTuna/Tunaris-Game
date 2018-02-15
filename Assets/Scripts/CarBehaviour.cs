﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CarBehaviour : NetworkBehaviour { 
    public Text speedDisplay;//output of speed to meter - by default MPH
    public Text gearDisplay;
    public RectTransform pointer;
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;
    public Transform wheelFLTrans;
    public Transform wheelFRTrans;
    public Transform wheelRLTrans;
    public Transform wheelRRTrans;
    public float currentSpeed;
    public float wheelRPM;

    public float currentGrip; //value manipulated by road type
    //tuneable stats
    public float brakeStrength = 200; //brake strength
    public float aero = 15.0f; //aero - higher value = higher grip, but less accel/topspeed
    public float ratio; //final drive
    public float tyreBias = 0.6f; //tyre bias - smaller value = offroad - higher value = tarmac (0-1) BASE TIRE GRIP VALUE = 1
    /// <summary>
    /// How much power is being sent to the front wheels, as a ratio, can be used to change drivetrain
    /// 0: no power to front, 100% power to rear
    /// 0.5: half front, half rear
    /// 1: 100% front,  no rear
    /// </summary>
    public float FrontWheelDriveBias = 0.5f;
    // Having it as a ratio opens a whole lot of tricks, but mainly an easy way to allocate power for ALL drivetrains
    // FrontPower = engineOutput * FrontWheelDriveBias
    // RearPower = engineOutput * (1-FrontWheelDriveBias)
    // Chaning this while the car is driving is an effective way of having a center diff


    //end tuneable stats
    public float engineRPM;
    public float engineREDLINE = 9000;//engine redline
    public float unitOutput;
    //gears
    public float gearR = -5.0f;
    public float gearN = 0.0f;
    public float gear1 = 5.4f;
    public float gear2 = 3.4f;
    public float gear3 = 2.7f;
    public float gear4 = 2.0f;
    public float gear5 = 1.8f;
    public float gear6 = 1.6f;
    private float[] gears;
    public int gear;//current gear
    public bool shifting = false;//shifter delay

    // CoG
    public Vector3 CenterOfGravity = new Vector3(0, -0.4f, 0.5f);

    void Start() {
        if (isLocalPlayer) {
            // Set the HUD objects
            CourseController ctrl = FindObjectOfType<CourseController>();
            speedDisplay = ctrl.SpeedDisplayHUD;
            gearDisplay = ctrl.GearDisplayHUD;
            pointer = ctrl.PointerHUD;

            // Set game camera target
            ctrl.Camera.GetComponent<CarCamera>().car = this.gameObject.transform;

            engineRPM = 800;
            ratio = 4.3f;
            Physics.gravity = new Vector3(0, -aero, 0);
            GetComponent<Rigidbody>().centerOfMass = CenterOfGravity;
            gear = 1;

            gears = new float[] { gearR, gearN, gear1, gear2, gear3, gear4, gear5, gear6 };

            HUDUpdate();
        }
    }

    void FixedUpdate() {
        if (isLocalPlayer) {
            StartCoroutine(engine());

            if (Input.GetAxis("Handbrake") > 0f)//HANDBRAKE
            {
                wheelRL.sidewaysFriction = SetStiffness(wheelRL.sidewaysFriction, 0.4f);
                wheelRR.sidewaysFriction = SetStiffness(wheelRR.sidewaysFriction, 0.4f);
                wheelRL.forwardFriction = SetStiffness(wheelRL.forwardFriction, 0.4f);
                wheelRR.forwardFriction = SetStiffness(wheelRR.sidewaysFriction, 0.4f);

                wheelRL.brakeTorque = 300;
                wheelRR.brakeTorque = 300;
            } else {
                wheelRL.brakeTorque = 0;
                wheelRR.brakeTorque = 0;
            }


            if (Input.GetAxis("Brake") > 0f) {//brakes
                wheelFL.brakeTorque = brakeStrength;
                wheelFR.brakeTorque = brakeStrength;
                wheelRL.brakeTorque = brakeStrength;
                wheelRR.brakeTorque = brakeStrength;
            } else {
                wheelFL.brakeTorque = 0;
                wheelFR.brakeTorque = 0;
                wheelRL.brakeTorque = 0;
                wheelRR.brakeTorque = 0;
            }

            wheelFR.steerAngle = 20 * Input.GetAxis("Steering");//steering
            wheelFL.steerAngle = 20 * Input.GetAxis("Steering");

            wheelRPM = (wheelFL.rpm + wheelRL.rpm) / 2; //speed counter
            currentSpeed = 2 * 22 / 7 * wheelFL.radius * wheelRL.rpm * 60 / 1000;
            currentSpeed = Mathf.Round(currentSpeed);
        }
    }

    WheelFrictionCurve SetStiffness(WheelFrictionCurve current, float newStiffness) {
        return new WheelFrictionCurve() {
            asymptoteSlip = wheelRL.sidewaysFriction.asymptoteSlip,
            asymptoteValue = wheelRL.sidewaysFriction.asymptoteValue,
            extremumSlip = wheelRL.sidewaysFriction.extremumSlip,
            extremumValue = wheelRL.sidewaysFriction.extremumValue,
            stiffness = newStiffness
        };
    }

    void Update() {
        if (isLocalPlayer) {
            StartCoroutine(gearbox());//gearbox update 
            HUDUpdate();
            SetSurfaceProperties();
            wheelFRTrans.Rotate(wheelFR.rpm / 60 * 360 * Time.deltaTime, 0, 0); //graphical updates
            wheelFLTrans.Rotate(wheelFL.rpm / 60 * 360 * Time.deltaTime, 0, 0);
            wheelRRTrans.Rotate(wheelRR.rpm / 60 * 360 * Time.deltaTime, 0, 0);
            wheelRLTrans.Rotate(wheelRL.rpm / 60 * 360 * Time.deltaTime, 0, 0);
            wheelFRTrans.localEulerAngles = new Vector3(wheelFRTrans.localEulerAngles.x, wheelFR.steerAngle - wheelFRTrans.localEulerAngles.z, wheelFRTrans.localEulerAngles.z);
            wheelFLTrans.localEulerAngles = new Vector3(wheelFLTrans.localEulerAngles.x, wheelFL.steerAngle - wheelFLTrans.localEulerAngles.z, wheelFLTrans.localEulerAngles.z);
            WheelPosition(); //graphical update - wheel positions 
        }
    }

    IEnumerator engine() {//engine
        if (gear == 1) {//neutral revs

            if (engineRPM >= engineREDLINE) {
                engineRPM = engineRPM - 300;
            } else {
                if (Input.GetAxis("Throttle") > 0) {
                    yield return new WaitForSeconds(0.1f);
                    engineRPM = 100 + engineRPM;
                } else {
                    if (engineRPM > 800) {
                        yield return new WaitForSeconds(0.05f);
                        engineRPM = engineRPM - 100;
                    }
                }


            }
        } else { //drive revs
            engineRPM = wheelRPM * gears[gear] * ratio + 800;
        }

        if (gear > 0) {
            unitOutput = (engineRPM / 1000) * (engineRPM / 1000) + 120; //ENGINE OUTPUT TO WHEELS
        } else {
            unitOutput = -(engineRPM / 1000) * (engineRPM / 1000) - 120; //reverse output
        }
        if (engineRPM > engineREDLINE || gear == 1 || Input.GetAxis("Throttle") < 0) {//throttle & rev limit
            wheelFR.motorTorque = 0 * FrontWheelDriveBias;
            wheelFL.motorTorque = 0 * FrontWheelDriveBias;

            wheelRR.motorTorque = 0 * (1 - FrontWheelDriveBias);
            wheelRL.motorTorque = 0 * (1 - FrontWheelDriveBias);
        } else {
            wheelFR.motorTorque = unitOutput * Input.GetAxis("Throttle") * FrontWheelDriveBias;
            wheelFL.motorTorque = unitOutput * Input.GetAxis("Throttle") * FrontWheelDriveBias;

            wheelRR.motorTorque = unitOutput * Input.GetAxis("Throttle") * (1 - FrontWheelDriveBias);
            wheelRL.motorTorque = unitOutput * Input.GetAxis("Throttle") * (1 - FrontWheelDriveBias);
        }
    }

    // Gearbox managed, called each frame
    IEnumerator gearbox() {
        if (Input.GetButtonDown("ShiftUp") == true && gear < 7 && shifting == false) {
            shifting = true;
            gear = gear + 1;
            yield return new WaitForSeconds(0.3f);
            shifting = false;
        }
        if (Input.GetButtonDown("ShiftDown") == true && gear > 0 && shifting == false) {
            shifting = true;
            gear = gear - 1;
            yield return new WaitForSeconds(0.1f);
            shifting = false;
        }
    }

    // Graphical update - wheel positions 
    void WheelPosition() {
        RaycastHit hit;
        Vector3 wheelPos;
        //FL
        if (Physics.Raycast(wheelFL.transform.position, -wheelFL.transform.up, out hit, wheelFL.radius + wheelFL.suspensionDistance)) {
            wheelPos = hit.point + wheelFL.transform.up * wheelFL.radius;
        } else {
            wheelPos = wheelFL.transform.position - wheelFL.transform.up * wheelFL.suspensionDistance;
        }
        wheelFLTrans.position = wheelPos;
        //FR
        if (Physics.Raycast(wheelFR.transform.position, -wheelFR.transform.up, out hit, wheelFR.radius + wheelFR.suspensionDistance)) {
            wheelPos = hit.point + wheelFR.transform.up * wheelFR.radius;
        } else {
            wheelPos = wheelFR.transform.position - wheelFR.transform.up * wheelFR.suspensionDistance;
        }
        wheelFRTrans.position = wheelPos;
        //RL
        if (Physics.Raycast(wheelRL.transform.position, -wheelRL.transform.up, out hit, wheelRL.radius + wheelRL.suspensionDistance)) {
            wheelPos = hit.point + wheelRL.transform.up * wheelRL.radius;
        } else {
            wheelPos = wheelRL.transform.position - wheelRL.transform.up * wheelRL.suspensionDistance;
        }
        wheelRLTrans.position = wheelPos;
        //RR
        if (Physics.Raycast(wheelRR.transform.position, -wheelRR.transform.up, out hit, wheelRR.radius + wheelRR.suspensionDistance)) {
            wheelPos = hit.point + wheelRR.transform.up * wheelRR.radius;
        } else {
            wheelPos = wheelRR.transform.position - wheelRR.transform.up * wheelRR.suspensionDistance;
        }
        wheelRRTrans.position = wheelPos;
    }

    // Sets the surface properties, based on what the front left (?) wheel is doing
    // Called each frame
    void SetSurfaceProperties() {
        WheelHit FLhit;
        float wheelpreload;

        // Ground surface detection
        if (wheelFL.GetGroundHit(out FLhit)) {
            if (FLhit.collider.gameObject.CompareTag("sand")) {
                if (currentGrip > 1.1f) {
                    currentGrip = 1.1f;
                } else {
                    currentGrip = 1 - tyreBias + 0.3f;//OFFROAD
                }
                // On sand, wheelpreload was set to 0.1f as constant
                wheelpreload = 0.1f;
            } else {
                if (currentGrip > 1.3f) {
                    currentGrip = 1.3f;
                } else {
                    currentGrip = 1 * tyreBias + 0.4f;//TARMAC
                }
                if (currentGrip > 0.9f)//drive wheel preload - provides extra grip to drive wheels.
                {//make sure to change application accordingly!!!! (RWD, FWD, AWD)
                    wheelpreload = 0f; //this setting applies to TARMAC ONLY!!! change offroad value accordingly
                } else {
                    wheelpreload = 0.2f;
                }
            }
            // Those are always called, might as well take them out of the if blocks
            // Front wheels
            wheelFR.sidewaysFriction = SetStiffness(wheelFR.sidewaysFriction, currentGrip + (wheelpreload * FrontWheelDriveBias));
            wheelFL.sidewaysFriction = SetStiffness(wheelFL.sidewaysFriction, currentGrip + (wheelpreload * FrontWheelDriveBias));
            wheelFR.forwardFriction = SetStiffness(wheelFL.forwardFriction, currentGrip + (wheelpreload * FrontWheelDriveBias));
            wheelFL.forwardFriction = SetStiffness(wheelFL.forwardFriction, currentGrip + (wheelpreload * FrontWheelDriveBias));
            // Rear wheels
            wheelRR.sidewaysFriction = SetStiffness(wheelRR.sidewaysFriction, currentGrip + (wheelpreload * (1 - FrontWheelDriveBias)));
            wheelRL.sidewaysFriction = SetStiffness(wheelRL.sidewaysFriction, currentGrip + (wheelpreload * (1 - FrontWheelDriveBias)));
            wheelRR.forwardFriction = SetStiffness(wheelRL.forwardFriction, currentGrip + (wheelpreload * (1 - FrontWheelDriveBias)));
            wheelRL.forwardFriction = SetStiffness(wheelRL.forwardFriction, currentGrip + (wheelpreload * (1 - FrontWheelDriveBias)));
        }
    }

    // Updates the HUD
    void HUDUpdate() {
        float speedFactor = engineRPM / engineREDLINE;//dial rotation
        float rotationAngle = 0;
        if (engineRPM >= 0) {
            rotationAngle = Mathf.Lerp(70, -160, speedFactor);
            pointer.eulerAngles = new Vector3(0, 0, rotationAngle);
        }//end dial rot

        if (currentSpeed < 0)//cancelling negative integers, speed
        {
            speedDisplay.text = (currentSpeed * -1).ToString();
        } else {
            speedDisplay.text = currentSpeed.ToString();
        }
        if (shifting) {
            gearDisplay.text = "-";
        } else {         
            //gears
            if (gear == 0) {
                gearDisplay.text = "R".ToString();//reverse gear
            } else if (gear == 1) {
                gearDisplay.text = "N".ToString();//neutral
            } else {
                gearDisplay.text = (gear - 1).ToString();//array value, minus 1
            }
        }
    }
}