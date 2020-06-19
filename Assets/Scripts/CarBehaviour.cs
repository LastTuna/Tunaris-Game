using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using SharpDX.DirectInput;
using System.Collections.Generic;
using System;

public class CarBehaviour : NetworkBehaviour {
    public EngineAudioBehaviour EngineAudio;
    public GameObject frontLights;
    public GameObject rearLights;
    //public Material dirt; //dirt MATERIAL.
    public float dirtiness;//start, call from savedata the dirtiness of the car, then apply
    //end of race will store and call to savedata to store dirtiness level
    public Transform drivingWheel;
    public WheelCollider wheelFL, wheelFR, wheelRL, wheelRR;
    public Transform wheelFLCalp, wheelFRCalp, wheelRLCalp, wheelRRCalp;
    public Transform wheelFLTrans, wheelFRTrans, wheelRLTrans, wheelRRTrans;
    public Transform differentialTrans;
    public bool visualDiff = false;
    public float currentSpeed, wheelRPM;
    public Material dirt; //dirt MATERIAL.
    public GameObject HUDPrefab;
    private HUD HUD;
    public int manual = 1; //0auto - 1manual - 2manualwclutch
    public float shifterDelay = 0.3f;
    //tuneable stats
    public float springStiffness = 8000;
    public float brakeStrength = 200; //brake strength
    public float aero = 5f;
    public float drag = 10f;
    public float ratio; //final drive
    public float maxSteerAngle = 20;
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
    public float lsd = 1f;

    //end tuneable stats
    public float airSpeed;
    public float engineRPM;
    public float engineIdle = 800;
    public float engineREDLINE = 9000;//engine redline - REDLINE AT 6000 IF TRUCK
    public AnimationCurve engineTorque = new AnimationCurve(new Keyframe(0, 130), new Keyframe(5000, 250), new Keyframe(9000, 200));//engine power - CHANGE TO 200 IF TRUCK
    public bool aspirated = false;
    public AnimationCurve turboWaste = new AnimationCurve(new Keyframe(0, 0), new Keyframe(5000, 1), new Keyframe(9000, 0)); //adjust turbo engagement curve, turbo pressure
    public float turboSpool = 0.1f;//turbo b00st
    public float turboSize = 10;//mm - adjusts turbo lag
    public bool spooled = false;//determine whether to play wastegate sound or not
    public float unitOutput;
    //clutch
    public float clutchPressure = 1; //0-1 clamp
    public float clutchRPM;
    //gears
    public float[] gears = new float[8] { -5.0f, 0.0f, 5.4f, 3.4f, 2.7f, 2.0f, 1.8f, 1.6f };
    public int gear;//current gear
    public bool shifting = false;//shifter delay

    public float debug1;
    public float debug2;
    public float debug3;
    public float debug4;


    // FFB shit
    private DeviceInstance steeringWheel;
    private Joystick steeringWheelJoy;
    private Effect ffbeffect;
    private EffectInfo constantForceEffect;
    private SharpDX.DirectInput.ConstantForce ffbForce;
    private int[] axes, dirs;

    public JointSpring springs = new JointSpring {
        //should load data from data controller when setup
        spring = 8000,
        damper = 800,
        targetPosition = 0.5f,
    };

    // CoG
    public Vector3 CenterOfGravity = new Vector3(0, -0.4f, 0.5f);

    // busrider mode
    private bool isBusrider;

    void Start() {
        if (isLocalPlayer && !isBusrider) {
            // Set game camera target
            CourseController ctrl = FindObjectOfType<CourseController>();
            ctrl.Camera.GetComponent<CarCamera>().car = this.gameObject.transform;

            // load the default HUD if no car HUD
            if (!HUDPrefab) {
                HUDPrefab = ctrl.DefaultHUD;
            }
            HUD = Instantiate(HUDPrefab, ctrl.HUDCanvas.transform).GetComponent<HUD>();

            if (!aspirated) turboSpool = 1;

            engineRPM = 800;
            GetComponent<Rigidbody>().centerOfMass = CenterOfGravity;
            gear = 1;

            wheelFR.ConfigureVehicleSubsteps(20, 1, 1);
            wheelFL.ConfigureVehicleSubsteps(20, 1, 1);
            wheelRL.ConfigureVehicleSubsteps(20, 1, 1);
            wheelRR.ConfigureVehicleSubsteps(20, 1, 1);

            DataController dataController = FindObjectOfType<DataController>();
            springStiffness = dataController.SpringStiffness;
            brakeStrength = dataController.BrakeStiffness;
            aero = dataController.Aero;
            ratio = dataController.FinalDrive;
            dirtiness = dataController.GetDirtiness();
            isBusrider = dataController.IsBusrider;

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

        if (isLocalPlayer && isBusrider) {
            CourseController ctrl = FindObjectOfType<CourseController>();
            HUDPrefab = ctrl.BusriderHUD;
            HUD = Instantiate(HUDPrefab, ctrl.HUDCanvas.transform).GetComponent<HUD>();

            ctrl.Camera.GetComponent<CarCameraBusrider>().car = this.gameObject.transform;
            ctrl.Camera.GetComponent<CarCamera>().enabled = false;

            AIBehaviour AI = gameObject.AddComponent<AIBehaviour>();
            AI.car = this;
            AI.courseController = ctrl;

            CustomInput.IsAI = true;
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
            AeroDrag();
            if (manual == 2) clutchPressure = 1 - CustomInput.GetAxis("Clutch");
            Engine();
            float targetSteering = maxSteerAngle * CustomInput.GetAxis("Steering");

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

            if(aspirated) Turbo();

            //currentSpeed = 2 * 22/7 * wheelFL.radius * wheelFL.rpm * 60 / 1000; legacy
            currentSpeed = 2 * 22 / 7 * wheelFL.radius * ((wheelFL.rpm + wheelFR.rpm) / 2 * FrontWheelDriveBias) * 60 / 1000;
            currentSpeed += 2 * 22 / 7 * wheelRL.radius * ((wheelRL.rpm + wheelRR.rpm) / 2 * (1 - FrontWheelDriveBias)) * 60 / 1000;
            currentSpeed = Mathf.Round(currentSpeed);
            Physics.gravity = new Vector3(0, -aero, 0);
        }
    }

    void Update() {
        if (isLocalPlayer) {
            HUD.UpdateHUD(engineRPM, engineREDLINE, currentSpeed, shifting, gear);

            Gearbox();//gearbox update
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

            EngineAudio.ProcessSounds(engineRPM, spooled, turboSpool);
            CmdLightsOn();
            WheelPosition(wheelFL, wheelFLCalp, wheelFLTrans); //graphical update - wheel positions
            WheelPosition(wheelFR, wheelFRCalp, wheelFRTrans);
            WheelPosition(wheelRL, wheelRLCalp, wheelRLTrans);
            WheelPosition(wheelRR, wheelRRCalp, wheelRRTrans);
            if (visualDiff) DiffPosition(wheelRRTrans, wheelRLTrans);
            drivingWheel.transform.localEulerAngles = new Vector3(drivingWheel.transform.rotation.x, drivingWheel.transform.rotation.y, -90 * CustomInput.GetAxis("Steering"));
            if (Input.GetButtonDown("Reset"))
            {
                Debug.Log("Reset was pressed");
                transform.rotation = Quaternion.identity;
                transform.position = new Vector3(transform.position.x, transform.position.y + 5, transform.position.z);
            }
            if (dirt)
            {
                dirt.SetFloat("_FortniteRange", dirtiness);
            }
        }
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
                engineRPM += engineTorque.Evaluate(engineRPM) * turboSpool * Mathf.Clamp01(CustomInput.GetAxis("Throttle"));
                if (engineRPM > engineIdle && Mathf.Clamp01(CustomInput.GetAxis("Throttle")) == 0) engineRPM -= engineTorque.Evaluate(engineRPM) - 100;
                if (engineRPM < engineIdle) engineRPM += 20;
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
            //make motor torks calc shorter with temporary variable
            float precalc = Mathf.Clamp01(CustomInput.GetAxis("Throttle")) * ((-0.5f + Mathf.Clamp01(gears[gear])) * 2) * clutchPressure * turboSpool;

            wheelFL.motorTorque = (engineTorque.Evaluate(engineRPM) - Differential(wheelFR, wheelFL)) * CenterDifferential(1) * FrontWheelDriveBias * precalc;
            wheelFR.motorTorque = (engineTorque.Evaluate(engineRPM) - Differential(wheelFL, wheelFR)) * CenterDifferential(1) * FrontWheelDriveBias * precalc;
            wheelRL.motorTorque = (engineTorque.Evaluate(engineRPM) - Differential(wheelRR, wheelRL)) * CenterDifferential(-1) * (1 - FrontWheelDriveBias) * precalc;
            wheelRR.motorTorque = (engineTorque.Evaluate(engineRPM) - Differential(wheelRL, wheelRR)) * CenterDifferential(-1) * (1 - FrontWheelDriveBias) * precalc;

            if (engineRPM >= engineREDLINE)
            {//rev limiter
                wheelFL.motorTorque = 0;
                wheelFR.motorTorque = 0;
                wheelRL.motorTorque = 0;
                wheelRR.motorTorque = 0;
            }
            if (engineRPM < engineIdle)
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
        
        wheelRPM = (wheelFL.rpm * 3.3f) * ratio; //speed counter
        airSpeed = Mathf.Abs(gameObject.GetComponent<Rigidbody>().velocity.x) +
            Mathf.Abs(gameObject.GetComponent<Rigidbody>().velocity.y) +
            Mathf.Abs(gameObject.GetComponent<Rigidbody>().velocity.z);
        
    }
    
    void Turbo()
    {
        if (CustomInput.GetAxis("Throttle") > 0 && engineRPM > engineIdle + 50)
        {
            turboSpool += turboWaste.Evaluate(engineRPM) / (turboSize * 3);
            if (turboSpool > turboWaste.Evaluate(engineRPM))
            {
                turboSpool = turboWaste.Evaluate(engineRPM);
                spooled = true;
            }
        }
        else
        {
            if (turboSpool > 0.2f)
            {
                spooled = false;
                turboSpool -= turboSpool / (turboSize * 4);
            }
        }
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
    public void Gearbox()
    {
        switch (manual)
        {
            case 0: //AUTOMATIC
                if(gear == 1)
                {
                    //neutral
                    if(engineRPM > engineIdle + 900) gear++;
                    break;
                }
                if(engineRPM > engineREDLINE - 500 && gear != gears.Length - 1)
                {
                    //upshift
                    gear++;
                    break;
                }
                if (engineRPM < engineREDLINE - engineREDLINE / 2 && gear != 2 && gear != 1)
                {
                    //downshift
                    gear--;
                    break;
                }
                if (engineRPM < engineIdle + 200 && gear == 2)
                {
                    //downshift to neutral
                    gear--;
                    break;
                }
                break;
            case 1: //MANUAL
                if (Input.GetButtonDown("ShiftUp") == true && gear < gears.Length - 1 && shifting == false)
                {
                    gear = gear + 1;
                    StartCoroutine(ClutchDelay(shifterDelay));
                }
                if (Input.GetButtonDown("ShiftDown") == true && gear > 0 && shifting == false)
                {
                    gear = gear - 1;
                    StartCoroutine(ClutchDelay(shifterDelay / 3));
                }
                break;
            case 2: //MANUAL CLUTCH
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
        }
    }
    public IEnumerator ClutchDelay(float time)
    {
        shifting = true;
        yield return new WaitForSeconds(time);
        shifting = false;
    }
    
    void AeroDrag ()
    {
        Vector3 drag = gameObject.transform.forward * -1 * (airSpeed * 0.1f);
        //Debug.Log(drag);
        //Debug.Log(transform.forward.y);
        gameObject.GetComponent<Rigidbody>().AddRelativeForce(drag, ForceMode.Force);

    }

    // Graphical update - wheel positions 
    void WheelPosition(WheelCollider wheel, Transform caliper, Transform visualWheel) {
        RaycastHit hit;
        Vector3 wheelPos;
        if (Physics.Raycast(wheel.transform.position, -wheel.transform.up, out hit, wheel.radius + wheel.suspensionDistance)) {
            wheelPos = hit.point + wheel.transform.up * wheel.radius;
            //UPDATE DIRTINESS
            if (hit.collider.gameObject.CompareTag("sand") && dirtiness < 1)
            {
                dirtiness += Mathf.Abs(wheelFL.rpm) / 500000;
            }
        } else {
            wheelPos = wheel.transform.position - wheel.transform.up * wheel.suspensionDistance;
        }
        caliper.position = wheelPos;
        visualWheel.position = wheelPos;
    }
    void DiffPosition(Transform rWheel, Transform lWheel)
    {
        Vector3 pos = new Vector3(0, (rWheel.localPosition.y + lWheel.localPosition.y) / 2 + 0.2f, rWheel.localPosition.z);
        differentialTrans.localPosition = pos;//apply transform
    }


    // start procedure
    public void PreRaceStart() {
        gear = 1;
        wheelFL.GetComponent<TireBehavior>().raceStartHandbrake = true;
        wheelFR.GetComponent<TireBehavior>().raceStartHandbrake = true;
        wheelRL.GetComponent<TireBehavior>().raceStartHandbrake = true;
        wheelRR.GetComponent<TireBehavior>().raceStartHandbrake = true;
    }

    public void RaceStart() {
        wheelFL.GetComponent<TireBehavior>().raceStartHandbrake = false;
        wheelFR.GetComponent<TireBehavior>().raceStartHandbrake = false;
        wheelRL.GetComponent<TireBehavior>().raceStartHandbrake = false;
        wheelRR.GetComponent<TireBehavior>().raceStartHandbrake = false;
    }

    [Command]
    void CmdLightsOn()
    {
        if (Input.GetButtonDown("Lights") && !frontLights.activeSelf)
        {
            frontLights.SetActive(true);
        }
        else if (Input.GetButtonDown("Lights") && frontLights.activeSelf)
        {
            frontLights.SetActive(false);
        }

        if (CustomInput.GetAxis("Brake") > 0f)
        {//brakes
            rearLights.SetActive(true);
        }
        else
        {
            rearLights.SetActive(false);
        }
    }
}