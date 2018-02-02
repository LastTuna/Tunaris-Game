using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class mustang : NetworkBehaviour {
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
    public Texture2D speedo;
    public Texture2D speedopoint;

    public float currentGrip = 1.3f;
    //tuneable stats
    public float brakeStrength = 200;
    public float aero = 15.0f;
    public float ratio;


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

    void Start() {
        // Initialize car camera
        engineRPM = 800;
        ratio = 4.3f;
        Physics.gravity = new Vector3(0, -aero, 0);
        GetComponent<Rigidbody>().centerOfMass = new Vector3(0, -0.4f, 0.5f);
        gear = 1;

        gears = new float[] { gearR, gearN, gear1, gear2, gear3, gear4, gear5, gear6 };

    }

    void FixedUpdate() {
        // Only process shit for the local player
        if (!isLocalPlayer) {
            return;
        }

        StartCoroutine(engine());

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
        if (Input.GetAxis("Handbrake") > 0f) {//HANDBRAKE
            wheelRL.sidewaysFriction = new WheelFrictionCurve() { stiffness = 0.5f };
            wheelRR.sidewaysFriction = new WheelFrictionCurve() { stiffness = 0.5f };
            wheelRL.forwardFriction = new WheelFrictionCurve() { stiffness = 0.5f };
            wheelRL.forwardFriction = new WheelFrictionCurve() { stiffness = 0.5f };

            wheelRL.brakeTorque = 300;
            wheelRR.brakeTorque = 300;
        } else {
            wheelRL.brakeTorque = 0;
            wheelRR.brakeTorque = 0;

            wheelRL.sidewaysFriction = new WheelFrictionCurve() { stiffness = currentGrip };
            wheelRR.sidewaysFriction = new WheelFrictionCurve() { stiffness = currentGrip };
            wheelRL.forwardFriction = new WheelFrictionCurve() { stiffness = currentGrip };
            wheelRL.forwardFriction = new WheelFrictionCurve() { stiffness = currentGrip };
        }

        wheelFR.steerAngle = 20 * Input.GetAxis("Steering");//steering
        wheelFL.steerAngle = 20 * Input.GetAxis("Steering");

        wheelRPM = (wheelFL.rpm + wheelRL.rpm) / 2; //speed counter
        currentSpeed = 2 * 22 / 7 * wheelFL.radius * wheelRL.rpm * 60 / 1000;
        currentSpeed = Mathf.Round(currentSpeed);
    }

    void Update() {
        // Only process shit for the local player
        if (!isLocalPlayer) {
            return;
        }

        StartCoroutine(gearbox());//gearbox update 

        wheelFRTrans.Rotate(wheelFL.rpm / 60 * 360 * Time.deltaTime, 0, 0); //graphical updates
        wheelFLTrans.Rotate(wheelFL.rpm / 60 * 360 * Time.deltaTime, 0, 0);
        wheelRRTrans.Rotate(wheelFL.rpm / 60 * 360 * Time.deltaTime, 0, 0);
        wheelRLTrans.Rotate(wheelFL.rpm / 60 * 360 * Time.deltaTime, 0, 0);
        wheelFRTrans.localEulerAngles = new Vector3(wheelFRTrans.localEulerAngles.x, wheelFR.steerAngle - wheelFRTrans.localEulerAngles.z);
        wheelFLTrans.localEulerAngles = new Vector3(wheelFLTrans.localEulerAngles.x, wheelFL.steerAngle - wheelFLTrans.localEulerAngles.z);
        //WheelPosition(); //graphical update - wheel positions 
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
            wheelFR.motorTorque = 0;
            wheelFL.motorTorque = 0;
        } else {
            wheelFR.motorTorque = unitOutput * Input.GetAxis("Throttle");
            wheelFL.motorTorque = unitOutput * Input.GetAxis("Throttle");
        }
    }
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

    void WheelPosition() { //graphical update - wheel positions 
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

    void OnGUI() {//dial
        GUI.DrawTexture(new Rect(Screen.width - 250, Screen.height - 160, 240, 160), speedo);
        float speedFactor = engineRPM / engineREDLINE;
        float rotationAngle = 0;
        if (engineRPM >= 0) {
            rotationAngle = Mathf.Lerp(-70, 160, speedFactor);
        }
        GUIUtility.RotateAroundPivot(rotationAngle, new Vector2(Screen.width - 162, Screen.height - 77));
        GUI.DrawTexture(new Rect(Screen.width - 215, Screen.height - 112, 100, 67), speedopoint);

    }
}