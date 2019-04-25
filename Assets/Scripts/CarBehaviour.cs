using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using SharpDX.DirectInput;
using System.Collections.Generic;
using System;

public class CarBehaviour : NetworkBehaviour {
    public EngineAudioBehaviour EngineAudio;

    public Text speedDisplay;//output of speed to meter - by default MPH
    public Text gearDisplay;
    public GameObject frontLights;
    public GameObject rearLights;
    //public Material dirt; //dirt MATERIAL.
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

    public int manual = 1; //0auto - 1manual - 2manualwclutch
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
    public float clutchPressure; //0-1 clamp
    public float clutchRPM;
    //gears
    public float[] gears = new float[8] { -5.0f, 0.0f, 5.4f, 3.4f, 2.7f, 2.0f, 1.8f, 1.6f };
    public int gear;//current gear
    public bool shifting = false;//shifter delay
    public int carIndex;

    public float debug1;
    public float debug2;
    public float debug3;

    // FFB shit
    private DeviceInstance steeringWheel;
    private Joystick steeringWheelJoy;
    private Effect ffbeffect;
    private EffectInfo constantForceEffect;
    private SharpDX.DirectInput.ConstantForce ffbForce;
    private int[] axes, dirs;

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
            GetComponent<Rigidbody>().centerOfMass = CenterOfGravity;
            gear = 1;

            //dirt = gameObject.transform.Find("DIRT").GetComponent<Renderer>().material;

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
            dirtiness = dataController.Dirtiness[carIndex];

            //spring stiffness set (dampers = spring / 10)
            springs.spring = springStiffness;
            springs.damper = springStiffness / 10;

            wheelFL.suspensionSpring = springs;
            wheelFR.suspensionSpring = springs;
            wheelRL.suspensionSpring = springs;
            wheelRR.suspensionSpring = springs;


            // Find an FFB wheel and hook it
            DirectInput directInput = new DirectInput();
            var ret = directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.ForceFeedback);
            if (ret.Count > 0) {
                steeringWheel = ret[0];
                steeringWheelJoy = new Joystick(directInput, steeringWheel.InstanceGuid);
                steeringWheelJoy.SetCooperativeLevel(GetWindowHandle(), CooperativeLevel.Exclusive | CooperativeLevel.Background);
                steeringWheelJoy.Properties.AutoCenter = true;


                steeringWheelJoy.Acquire();


                List<int> actuatorsObjectTypes = new List<int>();
                //Get all available force feedback actuators
                foreach (DeviceObjectInstance doi in steeringWheelJoy.GetObjects()) {
                    if ((doi.ObjectId.Flags & DeviceObjectTypeFlags.ForceFeedbackActuator) != 0)
                        actuatorsObjectTypes.Add((int)doi.ObjectId.Flags);
                }

                axes = new Int32[actuatorsObjectTypes.Count];
                int i = 0;
                foreach (int objt in actuatorsObjectTypes) {
                    axes[i++] = objt;
                }

                //Set effect direction
                dirs = new int[] { 1 };

                ffbForce = new SharpDX.DirectInput.ConstantForce {
                    Magnitude = 0
                };
                var ep = new EffectParameters {
                    Duration = -1,
                    Flags = EffectFlags.Cartesian | EffectFlags.ObjectIds,
                    Gain = 10000,
                    StartDelay = 0,
                    SamplePeriod = 0,
                    TriggerButton = -1,
                    TriggerRepeatInterval = 0
                };
                ep.SetAxes(axes, dirs);
                ep.Parameters = ffbForce;

                var allEffects = steeringWheelJoy.GetEffects();
                foreach (var effectInfo in allEffects) {
                    if (effectInfo.Name.Contains("Constant")) {
                        constantForceEffect = effectInfo;
                    }
                }

                ffbeffect = new Effect(steeringWheelJoy, constantForceEffect.Guid, ep);
                ffbeffect.Start();
            }
        }
    }

    /*** yolo 
     */
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern System.IntPtr GetActiveWindow();

    public static System.IntPtr GetWindowHandle() {
        return GetActiveWindow();
    }

    /* */

    void FixedUpdate() {
        if (isLocalPlayer) {
            if (manual == 2) clutchPressure = 1 - Input.GetAxis("Clutch");
            Engine();
            float targetSteering = 20 * Input.GetAxis("Steering");

            // FFB calculations
            if (ffbeffect != null) {

                WheelHit wheelHitR, wheelHitL;
                wheelFR.GetGroundHit(out wheelHitR);
                wheelFL.GetGroundHit(out wheelHitL);

                if (Input.GetButton("logiquake")) {
                    // meme
                    ffbForce.Magnitude = ffbForce.Magnitude < 0 ? 150000 : -150000;
                } else {
                    // non meme
                    // wheel weight calc
                    float wheelWeight = 1 - ((((wheelHitR.forwardSlip + wheelHitL.forwardSlip) / 2) + ((wheelHitR.sidewaysSlip + wheelHitL.sidewaysSlip) / 2))/2);
                    float centering = Vector3.SignedAngle(wheelHitR.forwardDir.normalized, GetComponent<Rigidbody>().transform.forward, Vector3.up);
                    float baseForce = (wheelHitR.force + wheelHitL.force) * 5;
                    ffbForce.Magnitude = (int) (baseForce * wheelWeight * centering * -0.5);
                }
                var ffbParams = new EffectParameters();
                ffbParams.Parameters = ffbForce;
                ffbeffect.SetParameters(ffbParams, EffectParameterFlags.TypeSpecificParameters);
            }

            //steering
            wheelFR.steerAngle = targetSteering;
            wheelFL.steerAngle = targetSteering;


            //currentSpeed = 2 * 22/7 * wheelFL.radius * wheelFL.rpm * 60 / 1000; legacy
            currentSpeed = 2 * 22 / 7 * wheelFL.radius * ((wheelFL.rpm + wheelFR.rpm) / 2 * FrontWheelDriveBias) * 60 / 1000;
            currentSpeed += 2 * 22 / 7 * wheelRL.radius * ((wheelRL.rpm + wheelRR.rpm) / 2 * (1 - FrontWheelDriveBias)) * 60 / 1000;
            currentSpeed = Mathf.Round(currentSpeed);
            Physics.gravity = new Vector3(0, -aero, 0);
        }
    }

    void Update() {
        if (isLocalPlayer) {
            HUDUpdate();
            StartCoroutine(Gearbox());//gearbox update
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

            EngineAudio.ProcessSounds(engineRPM, spooled);
            LightsOn();
            WheelPosition(); //graphical update - wheel positions 
            drivingWheel.transform.localEulerAngles = new Vector3(drivingWheel.transform.rotation.x, drivingWheel.transform.rotation.y, -90 * Input.GetAxis("Steering"));
            if (Input.GetButtonDown("Reset"))
            {
                Debug.Log("Reset was pressed");
                transform.rotation = Quaternion.identity;
                transform.position = new Vector3(transform.position.x, transform.position.y + 5, transform.position.z);
            }

        }
        //dirt.color = new Color(1,1,1, dirtiness);
    }

    void Engine()
    {//engine
        if(gear == 1 || clutchPressure < 0.5f)
        {//NEUTRAL
            if (engineRPM >= engineREDLINE)
            {
                engineRPM -= 300;
            }
            else
            {
                engineRPM += engineTorque.Evaluate(engineRPM) * Mathf.Clamp01(Input.GetAxis("Throttle"));
                if (engineRPM > 800 && Mathf.Clamp01(Input.GetAxis("Throttle")) == 0) engineRPM -= engineTorque.Evaluate(engineRPM) - 100;
                if (engineRPM < 800) engineRPM += 20;
            }
            clutchRPM = engineRPM;
            wheelFL.motorTorque = 0;
            wheelFR.motorTorque = 0;
            wheelRL.motorTorque = 0;
            wheelRR.motorTorque = 0;
        }
        else
        {//DRIVE
            engineRPM = (Mathf.Abs(wheelFL.rpm + wheelFR.rpm) / 2 * FrontWheelDriveBias) * ratio * Mathf.Abs(gears[gear]);
            engineRPM += (Mathf.Abs(wheelRL.rpm + wheelRR.rpm) / 2 * (1 - FrontWheelDriveBias)) * ratio * Mathf.Abs(gears[gear]);



            //currentSpeed += 2 * 22 / 7 * wheelRL.radius * ((wheelRL.rpm + wheelRR.rpm) / 2 * (1 - FrontWheelDriveBias)) * 60 / 1000;
            wheelFL.motorTorque = (engineTorque.Evaluate(engineRPM) - Differential(wheelFR, wheelFL)) * CenterDifferential(1) * FrontWheelDriveBias * Mathf.Clamp01(Input.GetAxis("Throttle")) * ((-0.5f + Mathf.Clamp01(gears[gear])) * 2) * clutchPressure;
            wheelFR.motorTorque = (engineTorque.Evaluate(engineRPM) - Differential(wheelFL, wheelFR)) * CenterDifferential(1) * FrontWheelDriveBias * Mathf.Clamp01(Input.GetAxis("Throttle")) * ((-0.5f + Mathf.Clamp01(gears[gear])) * 2) * clutchPressure;
            wheelRL.motorTorque = (engineTorque.Evaluate(engineRPM) - Differential(wheelRR, wheelRL)) * CenterDifferential(-1) * (1 - FrontWheelDriveBias) * Mathf.Clamp01(Input.GetAxis("Throttle")) * ((-0.5f + Mathf.Clamp01(gears[gear])) * 2) * clutchPressure;
            wheelRR.motorTorque = (engineTorque.Evaluate(engineRPM) - Differential(wheelRL, wheelRR)) * CenterDifferential(-1) * (1 - FrontWheelDriveBias) * Mathf.Clamp01(Input.GetAxis("Throttle")) * ((-0.5f + Mathf.Clamp01(gears[gear])) * 2) * clutchPressure;

            if (engineRPM >= engineREDLINE)
            {//rev limiter
                wheelFL.motorTorque = 0;
                wheelFR.motorTorque = 0;
                wheelRL.motorTorque = 0;
                wheelRR.motorTorque = 0;
            }
            if (engineRPM < 800)
            {//idling
                wheelFL.motorTorque = 100 * FrontWheelDriveBias * ((-0.5f + Mathf.Clamp01(gears[gear])) * 2) * clutchPressure;
                wheelFR.motorTorque = 100 * FrontWheelDriveBias * ((-0.5f + Mathf.Clamp01(gears[gear])) * 2) * clutchPressure;
                wheelRL.motorTorque = 100 * (1 - FrontWheelDriveBias) * ((-0.5f + Mathf.Clamp01(gears[gear])) * 2) * clutchPressure;
                wheelRR.motorTorque = 100 * (1 - FrontWheelDriveBias) * ((-0.5f + Mathf.Clamp01(gears[gear])) * 2) * clutchPressure;
            }
            if (clutchRPM > engineRPM)
            {//clutch kick
                clutchRPM -= engineTorque.Evaluate(engineRPM);
                engineRPM = clutchRPM;
                wheelFL.motorTorque = 300 * FrontWheelDriveBias * ((-0.5f + Mathf.Clamp01(gears[gear])) * 2) * clutchPressure;
                wheelFR.motorTorque = 300 * FrontWheelDriveBias * ((-0.5f + Mathf.Clamp01(gears[gear])) * 2) * clutchPressure;
                wheelRL.motorTorque = 300 * (1 - FrontWheelDriveBias) * ((-0.5f + Mathf.Clamp01(gears[gear])) * 2) * clutchPressure;
                wheelRR.motorTorque = 300 * (1 - FrontWheelDriveBias) * ((-0.5f + Mathf.Clamp01(gears[gear])) * 2) * clutchPressure;
            }
            else if (clutchRPM + 300 < engineRPM)
            {
                clutchRPM = 0;
            }

        }
        //if((wheelFL.rpm * ratio) * gears[gear] < engineRPM)

        debug1 = Differential(wheelFR, wheelFL);
        debug2 = CenterDifferential(1);
        debug3 = CenterDifferential(-1);
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
    }

    public float CenterDifferential (float front)
    {
        //if car is not awd, dont calculate center diff (DUH!)
        if (FrontWheelDriveBias == 0 || FrontWheelDriveBias == 1)
        {
            return 1;
        }
        else
        {
            //deduct 50% of power if wheels are faster than fronts
            if (front * ((wheelFL.rpm + wheelFR.rpm) / 2) - ((wheelRL.rpm + wheelRR.rpm) / 2) > lsd)
            {
                return 0.5f;
            }
            else
            {
                return 1;
            }

        }
    }

    public float Differential(WheelCollider left, WheelCollider right)
    {
        //deduct power from inner tire to keep tyres in sync, but without cutting power completely
        //if difference is lower than 0 (negative) return 0.

        if(Mathf.Abs(right.rpm) - Mathf.Abs(left.rpm) < 0)
        {
            return 0;
        }
        else
        {
            return Mathf.Abs(right.rpm) - Mathf.Abs(left.rpm);
        }
    }

    // Gearbox managed, called each frame
    IEnumerator Gearbox()
    {
        switch (manual)
        {
            case 0:
                //ADD AUTOMATIC CODE
                break;
            case 1:
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
                break;
            case 2:
                if (Input.GetButtonDown("ShiftUp") == true && gear < gears.Length - 1 && clutchPressure < 0.5f)
                {
                    shifting = true;
                    gear = gear + 1;
                    shifting = false;
                }
                if (Input.GetButtonDown("ShiftDown") == true && gear > 0 && clutchPressure < 0.5f)
                {
                    shifting = true;
                    gear = gear - 1;
                    shifting = false;
                }
                break;

        }//END MANUAL
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
            rotationAngle = Mathf.Lerp(90, -180, speedFactor);
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