using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CarBehaviour : NetworkBehaviour {
    public EngineAudioBehaviour EngineAudio;

    public Text speedDisplay;//output of speed to meter - by default MPH
    public Text gearDisplay;
    public GameObject frontLights;
    public GameObject rearLights;
    public Material dirt; //dirt MATERIAL.
    public Renderer dirtMesh; //fetches and instantiates dirt material
    public float dirtiness;//private int, start, call from savedata the dirtiness of the car, then apply
    //end of race will store and call to savedata to store dirtiness level
    public Transform drivingWheel;
    public RectTransform pointer;
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;

    public Transform wheelFLCalp;
    public Transform wheelFRCalp;
    public Transform wheelRLCalp;
    public Transform wheelRRCalp;

    public Transform wheelFLTrans;
    public Transform wheelFRTrans;
    public Transform wheelRLTrans;
    public Transform wheelRRTrans;
    public float currentSpeed;
    public float wheelRPM;

    public bool manual = false; //manual(true) auto(false)
    public float currentGrip; //value manipulated by road type
    //tuneable stats
    public float springStiffness = 8000;
    public float brakeStrength = 200; //brake strength
    public float aero = 15.0f; //aero - higher value = higher grip, but less accel/topspeed
    public float ratio; //final drive
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
    public float lsd = 0.5f;

    //end tuneable stats
    public float airSpeed;
    public float engineRPM;
    public float engineREDLINE = 9000;//engine redline - REDLINE AT 6000 IF TRUCK
    public AnimationCurve engineTorque = new AnimationCurve(new Keyframe(0, 130), new Keyframe(5000, 250), new Keyframe(9000, 200));//engine power - CHANGE TO 200 IF TRUCK
    public float turboSpool = 0.1f;//turbo boost value
    public bool spooled = false;//determine whether to play wastegate sound or not
    public float unitOutput;
    //clutch
    public float clutchPressure;
    public float clutchRPM;
    //gears
    public float[] gears = new float[8] { -5.0f, 0.0f, 5.4f, 3.4f, 2.7f, 2.0f, 1.8f, 1.6f };
    public int gear;//current gear
    public bool shifting = false;//shifter delay
    public int carIndex;

    public float debug1;
    public float debug2;
    public float debug3;

    public JointSpring springs = new JointSpring {
        spring = 8000,
        damper = 800,
        targetPosition = 0.1f,
    };

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
            Physics.gravity = new Vector3(0, -aero, 0);
            GetComponent<Rigidbody>().centerOfMass = CenterOfGravity;
            gear = 1;
            dirt = dirtMesh.GetComponent<Renderer>().material;

            wheelFR.ConfigureVehicleSubsteps(20, 30, 10);
            wheelFL.ConfigureVehicleSubsteps(20, 30, 10);
            wheelRL.ConfigureVehicleSubsteps(20, 30, 10);
            wheelRR.ConfigureVehicleSubsteps(20, 30, 10);


            HUDUpdate();
            //stats update

            DataController dataController = FindObjectOfType<DataController>();
            springStiffness = dataController.SpringStiffness;
            brakeStrength = dataController.BrakeStiffness;
            aero = dataController.Aero;
            ratio = dataController.FinalDrive;
            manual = true;
            dirtiness = dataController.Dirtiness[carIndex];

            //spring stiffness set (dampers = spring / 10)
            springs.spring = springStiffness;
            springs.damper = springStiffness / 10;

            wheelFL.suspensionSpring = springs;
            wheelFR.suspensionSpring = springs;
            wheelRL.suspensionSpring = springs;
            wheelRR.suspensionSpring = springs;
        }
    }

    void FixedUpdate() {
        if (isLocalPlayer) {
            Engine();

            if (Input.GetButtonDown("Reset")) {
                Debug.Log("Reset was pressed");
                transform.rotation = Quaternion.identity;
                transform.position = new Vector3(transform.position.x, transform.position.y + 5, transform.position.z);
            }



            wheelFR.steerAngle = 20 * Input.GetAxis("Steering");//steering
            wheelFL.steerAngle = 20 * Input.GetAxis("Steering");

            currentSpeed = 2 * 22 / 7 * wheelFL.radius * wheelRL.rpm * 60 / 1000;
            currentSpeed = Mathf.Round(currentSpeed);
        }
    }

    void Update() {
        if (isLocalPlayer) {
            StartCoroutine(Gearbox());//gearbox update 
            HUDUpdate();
            wheelFRTrans.Rotate(wheelFR.rpm / 60 * 360 * Time.deltaTime, 0, 0); //graphical updates
            wheelFLTrans.Rotate(wheelFL.rpm / 60 * 360 * Time.deltaTime, 0, 0);
            wheelRRTrans.Rotate(wheelRR.rpm / 60 * 360 * Time.deltaTime, 0, 0);
            wheelRLTrans.Rotate(wheelRL.rpm / 60 * 360 * Time.deltaTime, 0, 0);
            wheelFRTrans.localEulerAngles = new Vector3(wheelFRTrans.localEulerAngles.x, wheelFR.steerAngle - wheelFRTrans.localEulerAngles.z, wheelFRTrans.localEulerAngles.z);
            wheelFLTrans.localEulerAngles = new Vector3(wheelFLTrans.localEulerAngles.x, wheelFL.steerAngle - wheelFLTrans.localEulerAngles.z, wheelFLTrans.localEulerAngles.z);
            wheelRRTrans.localEulerAngles = new Vector3(wheelRRTrans.localEulerAngles.x, wheelRR.steerAngle - wheelRRTrans.localEulerAngles.z, wheelRRTrans.localEulerAngles.z);
            wheelRLTrans.localEulerAngles = new Vector3(wheelRLTrans.localEulerAngles.x, wheelRL.steerAngle - wheelRLTrans.localEulerAngles.z, wheelRLTrans.localEulerAngles.z);

            wheelFLCalp.localEulerAngles = new Vector3(wheelFLCalp.localEulerAngles.x, wheelFL.steerAngle - wheelFLCalp.localEulerAngles.z, wheelFLCalp.localEulerAngles.z);
            wheelFRCalp.localEulerAngles = new Vector3(wheelFRCalp.localEulerAngles.x, wheelFR.steerAngle - wheelFRCalp.localEulerAngles.z, wheelFRCalp.localEulerAngles.z);
            wheelRLCalp.localEulerAngles = new Vector3(wheelRLCalp.localEulerAngles.x, wheelRL.steerAngle - wheelRLCalp.localEulerAngles.z, wheelRLCalp.localEulerAngles.z);
            wheelRRCalp.localEulerAngles = new Vector3(wheelRRCalp.localEulerAngles.x, wheelRR.steerAngle - wheelRRCalp.localEulerAngles.z, wheelRRCalp.localEulerAngles.z);


            LightsOn();
            WheelPosition(); //graphical update - wheel positions 
            drivingWheel.transform.localEulerAngles = new Vector3(drivingWheel.transform.rotation.x, drivingWheel.transform.rotation.y, -90 * Input.GetAxis("Steering"));
        }
        dirt.color = new Color(1,1,1, dirtiness);
    }

    void Engine()
    {//engine
        if (engineRPM >= engineREDLINE)
        {
            engineRPM -= 300;
        }
        else
        {
            engineRPM += engineTorque.Evaluate(engineRPM) * Input.GetAxis("Throttle");
            if (engineRPM > 800 && Input.GetAxis("Throttle") == 0) engineRPM -= engineTorque.Evaluate(engineRPM) - 100;
            if (engineRPM < 800) engineRPM += 20;
        }

        if((wheelFL.rpm * ratio) * gears[gear] < engineRPM)
        {
            wheelFL.motorTorque = engineTorque.Evaluate(engineRPM);
            wheelFR.motorTorque = engineTorque.Evaluate(engineRPM);
        }
        else
        {
            wheelFL.motorTorque = 0;
            wheelFR.motorTorque = 0;
        }
        wheelRPM = (wheelFL.rpm * 3.3f) * ratio; //speed counter
        airSpeed = Mathf.Abs(gameObject.GetComponent<Rigidbody>().velocity.x) +
            Mathf.Abs(gameObject.GetComponent<Rigidbody>().velocity.y) +
            Mathf.Abs(gameObject.GetComponent<Rigidbody>().velocity.z);
    }


    void Turbo()
    { 

        if (engineRPM > 830 && Input.GetAxis("Throttle") > 0)
        {//when you step on the gas

            if (turboSpool < 1.8f)//contol to keep boost level tops at 1.8
            {
                turboSpool = turboSpool + 0.1f * (turboSpool / 2);
            }
            if (turboSpool > 1.3f)//after boost exceeds 1.3, play wastegate
            {
                spooled = true;
            }
        }
        else
        {
            turboSpool = 0.1f;
            spooled = false;
        }
        //SOUND UPDATES
        EngineAudio.ProcessSounds(engineRPM, spooled);
    }

    public float Differential (WheelCollider left, WheelCollider right)
    {
        if((left.rpm - right.rpm) * lsd > 0 && (left.rpm - right.rpm) * lsd < right.rpm)
        {
            return (left.rpm - right.rpm) * lsd;
        }
        else
        {
            return 0;
        }
    }

    // Gearbox managed, called each frame
    IEnumerator Gearbox()
    {
        if (manual)
        {
            if (Input.GetButtonDown("ShiftUp") == true && gear < gears.Length - 1 && shifting == false)
            {
                shifting = true;
                gear = gear + 1;
                yield return new WaitForSeconds(0.3f);
                shifting = false;
            }
            if (Input.GetButtonDown("ShiftDown") == true && gear > 0 && shifting == false)
            {
                shifting = true;
                gear = gear - 1;
                yield return new WaitForSeconds(0.1f);
                shifting = false;
            }
        }//END MANUAL
        else
        {
            {//START OF AUTOMATIC TRANS
                if (engineRPM > (engineREDLINE - 1000) && gear < 7 && shifting == false)//upshift
                {
                    shifting = true;
                    gear = gear + 1;
                    yield return new WaitForSeconds(0.3f);
                    shifting = false;
                }//end upshift
                if (engineRPM < 5000 && gear > 0 && shifting == false && currentSpeed > 30)//downshift
                {
                    shifting = true;
                    gear = gear - 1;
                    yield return new WaitForSeconds(0.1f);
                    shifting = false;


                } //downshiftEND

            }
        }
    }
    

    // Graphical update - wheel positions 
    void WheelPosition() {
        RaycastHit hit;
        Vector3 wheelPos;
        //FL
        if (Physics.Raycast(wheelFL.transform.position, -wheelFL.transform.up, out hit, wheelFL.radius + wheelFL.suspensionDistance)) {
            wheelPos = hit.point + wheelFL.transform.up * wheelFL.radius;
            //UPDATE DIRTINESS
            if (hit.collider.gameObject.CompareTag("sand") && dirtiness < 1)
            {
                dirtiness += Mathf.Abs(wheelFL.rpm) / 500000;
            }
        } else {
            wheelPos = wheelFL.transform.position - wheelFL.transform.up * wheelFL.suspensionDistance;
        }
        wheelFLCalp.position = wheelPos;
        wheelFLTrans.position = wheelPos;
        //FR
        if (Physics.Raycast(wheelFR.transform.position, -wheelFR.transform.up, out hit, wheelFR.radius + wheelFR.suspensionDistance)) {
            wheelPos = hit.point + wheelFR.transform.up * wheelFR.radius;
        } else {
            wheelPos = wheelFR.transform.position - wheelFR.transform.up * wheelFR.suspensionDistance;
        }
        wheelFRCalp.position = wheelPos;
        wheelFRTrans.position = wheelPos;
        //RL
        if (Physics.Raycast(wheelRL.transform.position, -wheelRL.transform.up, out hit, wheelRL.radius + wheelRL.suspensionDistance)) {
            wheelPos = hit.point + wheelRL.transform.up * wheelRL.radius;
        } else {
            wheelPos = wheelRL.transform.position - wheelRL.transform.up * wheelRL.suspensionDistance;
        }
        wheelRLCalp.position = wheelPos;
        wheelRLTrans.position = wheelPos;
        //RR
        if (Physics.Raycast(wheelRR.transform.position, -wheelRR.transform.up, out hit, wheelRR.radius + wheelRR.suspensionDistance)) {
            wheelPos = hit.point + wheelRR.transform.up * wheelRR.radius;
        } else {
            wheelPos = wheelRR.transform.position - wheelRR.transform.up * wheelRR.suspensionDistance;
        }
        wheelRRCalp.position = wheelPos;
        wheelRRTrans.position = wheelPos;
        
    }

    void LightsOn()
    {
        if (Input.GetButtonDown("Lights") && !frontLights.activeSelf)
        {
            frontLights.SetActive(true);
        }
        else if (Input.GetButtonDown("Lights") && frontLights.activeSelf)
        {
            frontLights.SetActive(false);
        }

        if (Input.GetAxis("Brake") > 0f && frontLights.activeSelf)
        {//brakes
            rearLights.SetActive(true);
        }
        else
        {
            rearLights.SetActive(false);
        }
    }

    // Updates the HUD
    void HUDUpdate() {
        float speedFactor = engineRPM / engineREDLINE;//dial rotation
        float rotationAngle = 0;
        if (engineRPM >= 0) {
            rotationAngle = Mathf.Lerp(90, -160, speedFactor);
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