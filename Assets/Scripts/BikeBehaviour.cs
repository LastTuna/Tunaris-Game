using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BikeBehaviour : MonoBehaviour {

    public Vector3 centerofgrav = new Vector3(0, -2f, 0);
    public Vector3 zVelocity;
    public WheelCollider wheelF;
    public WheelCollider wheelR;
    public Transform wheelFTrans;
    public Transform wheelRTrans;
    public Transform fork;
    public float power;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        
        wheelF.steerAngle = 6 * Input.GetAxis("Steering");

            wheelR.motorTorque = power * Input.GetAxis("Throttle");
        

        zVelocity = gameObject.GetComponent<Rigidbody>().angularVelocity;

        wheelFTrans.Rotate(wheelF.rpm / 60 * 360 * Time.deltaTime, 0, 0); //graphical updates
        wheelRTrans.Rotate(wheelR.rpm / 60 * 360 * Time.deltaTime, 0, 0);
        wheelFTrans.localEulerAngles = new Vector3(wheelFTrans.localEulerAngles.x, wheelF.steerAngle - wheelFTrans.localEulerAngles.z, wheelFTrans.localEulerAngles.z);
        wheelRTrans.localEulerAngles = new Vector3(wheelRTrans.localEulerAngles.x, wheelR.steerAngle - wheelRTrans.localEulerAngles.z, wheelRTrans.localEulerAngles.z);

        WheelPosition(); //graphical update - wheel positions 


        if (Input.GetAxis("Brake") > 0f)
        {//brakes
            wheelF.brakeTorque = 20;
            wheelR.brakeTorque = 10;
        }
        else
        {
            wheelF.brakeTorque = 0;
            wheelR.brakeTorque = 0;
        }
        WeightTransfer();
        AngBike();
    }

    public void WeightTransfer()
    {
        centerofgrav = new Vector3(0, centerofgrav.y, -Input.GetAxis("WeightTransfer") / 3);
        gameObject.GetComponent<Rigidbody>().centerOfMass = centerofgrav;
        //add weight transfer damping

    }


    // Graphical update - wheel positions 
    void WheelPosition()
    {
        RaycastHit hit;
        Vector3 wheelPos;
        //F
        if (Physics.Raycast(wheelF.transform.position, -wheelF.transform.up, out hit, wheelF.radius + wheelF.suspensionDistance))
        {
            wheelPos = hit.point + wheelF.transform.up * wheelF.radius;
        }
        else
        {
            wheelPos = wheelF.transform.position - wheelF.transform.up * wheelF.suspensionDistance;
        }
        wheelFTrans.position = wheelPos;
        //R
        if (Physics.Raycast(wheelR.transform.position, -wheelR.transform.up, out hit, wheelR.radius + wheelR.suspensionDistance))
        {
            wheelPos = hit.point + wheelR.transform.up * wheelR.radius;
        }
        else
        {
            wheelPos = wheelR.transform.position - wheelR.transform.up * wheelR.suspensionDistance;
        }
        wheelRTrans.position = wheelPos;
    }

    public void AngBike()
    {
        //the 2 raycasts are basically one at 0° and another at 180°,
        // if it hits it changes, it's not that far away from the bike so it touches when you're turning or
        // when you're close to something
        RaycastHit hit;
        if (Physics.Raycast(wheelF.transform.position, wheelF.transform.right, out hit,1) || (Physics.Raycast(wheelF.transform.position, -wheelF.transform.right, out hit, 1)))
        {
            gameObject.GetComponent<Rigidbody>().angularDrag = 4;
        }
        else
        {
            gameObject.GetComponent<Rigidbody>().angularDrag = 2;
        }
    }





}
