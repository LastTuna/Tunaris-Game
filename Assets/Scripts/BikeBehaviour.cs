using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BikeBehaviour : MonoBehaviour {

    public Vector3 centerofgrav = new Vector3(0, -2f, 0);
    public Vector3 zVelocity;
    public float Turn;
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
        float MaxTurn = 0.55f;
        // This is a very hacky way (i guess) to fix the bike going apeshit issue
        // the bike should be relatively stable, if not try adjusting MaxTurn (default 0.55f)
        // or the angularDrag value
        Turn = Mathf.Abs(zVelocity.y);
        if (Turn >= MaxTurn)
        {
            gameObject.GetComponent<Rigidbody>().angularDrag = 3.5f;
        }
        else
        {
            gameObject.GetComponent<Rigidbody>().angularDrag = 2;
        }
        Debug.Log(gameObject.GetComponent<Rigidbody>().angularDrag);
    }





}
