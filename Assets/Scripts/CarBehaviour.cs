using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SharpDX.DirectInput;
using System.Collections.Generic;
using System;

public class CarBehaviour : MonoBehaviour {
    public EngineAudioBehaviour EngineAudio;
    public GameObject frontLights;
    public GameObject rearLights;
    public Transform wheelFRTrans, wheelFLTrans, wheelRRTrans, wheelRLTrans;//needed for moving rear diff
    public float dirtiness;//start, call from savedata the dirtiness of the car, then apply
    //end of race will store and call to savedata to store dirtiness level
    public Transform drivingWheel;
    public WheelCollider wheelFL, wheelFR, wheelRL, wheelRR;
    public Transform differentialTrans;
    public float currentSpeed, wheelRPM;
    public Material dirt; //dirt MATERIAL.
    public int manual = 1; //0auto - 1manual - 2manualwclutch
    //tuneable stats
    public CarData specs;
    public HUD Hud;//HUD container
    
    /// <summary>
    /// How much power is being sent to the front wheels, as a ratio, can be used to change drivetrain
    /// 0: no power to front, 100% power to rear
    /// 0.5: half front, half rear
    /// 1: 100% front,  no rear
    /// </summary>
    // Having it as a ratio opens a whole lot of tricks, but mainly an easy way to allocate power for ALL drivetrains
    // FrontPower = engineOutput * FrontWheelDriveBias
    // RearPower = engineOutput * (1-FrontWheelDriveBias)
    // Chaning this while the car is driving is an effective way of having a center diff

    //end tuneable stats
    public float engineOUT;//engine output
    public float airSpeed;
    public float engineRPM;
    public float turboSpool = 1;
    public bool spooled = false;//determine whether to play wastegate sound or not
    public float unitOutput;//current power output to wheels
    //clutch
    public float clutchPressure = 1; //0-1 clamp
    public float clutchRPM;
    //gears
    public int gear;//current gear
    public bool shifting = false;//shifter delay

    public float debug1;
    public float debug2;
    public float debug3;
    private float[] tirewearlist;

    // FFB shit
    private DeviceInstance steeringWheel;
    private Joystick steeringWheelJoy;
    private Effect ffbeffect;
    private EffectInfo constantForceEffect;
    private SharpDX.DirectInput.ConstantForce ffbForce;
    private int[] axes, dirs;
    public float FFBMultiplier = 1.0f;

    
    // busrider mode
    private bool isBusrider;

    void Start() {
        if (!isBusrider)
        {
            DataController dataController = FindObjectOfType<DataController>();
            ContentManager cm = FindObjectOfType<ContentManager>();

            AssetBundle cor = cm.GetCar(dataController.SelectedCar);
            //initialize car data object.
            specs = new CarData();
            //LOAD CAR SPECS. if no car specs are found, run defaults.
            TextAsset SpecData = cor.LoadAsset("specs.json") as TextAsset;
            if(SpecData != null)
            {
                specs = specs.ImportData(SpecData.text);
            }

            //instantiate HUD. Then get the specific HUD data.
            TextAsset hudData = cor.LoadAsset("HUD.json") as TextAsset;

            if(hudData == null)
            {
                //default HUD
                GameObject hudprefab = Instantiate(cm.DefaultHUD);
                Hud = hudprefab.AddComponent<HUD>();
                Hud.GetUIinfo("default");
            }
            else
            {
                //CUSTOM HUD
                GameObject hudprefab = Instantiate(cor.LoadAsset("HUD") as GameObject);
                Hud = hudprefab.AddComponent<HUD>();
                Hud.GetUIinfo(hudData.text);
            }

            InitializeCar();
            // Set game camera target
            CourseController ctrl = FindObjectOfType<CourseController>();
            ctrl.Camera.GetComponent<CarCamera>().car = this.gameObject.transform;

            tirewearlist = new float[] {100,100,100,100};
            
            engineRPM = 800;
            GetComponent<Rigidbody>().centerOfMass = specs.centerOfGravy;
            GetComponent<Rigidbody>().mass = specs.weight;
            gear = 1;

            wheelFR.ConfigureVehicleSubsteps(5, 12, 15);
            wheelFL.ConfigureVehicleSubsteps(5, 12, 15);
            wheelRL.ConfigureVehicleSubsteps(5, 12, 15);
            wheelRR.ConfigureVehicleSubsteps(5, 12, 15);

            dirtiness = dataController.GetDirtiness();
            isBusrider = dataController.IsBusrider;
            //call the FFB start routine
            FFBStart();

        }
        if (isBusrider) {
            CourseController ctrl = FindObjectOfType<CourseController>();

            ctrl.Camera.GetComponent<CarCameraBusrider>().car = this.gameObject.transform;
            ctrl.Camera.GetComponent<CarCamera>().enabled = false;

            AIBehaviour AI = gameObject.AddComponent<AIBehaviour>();
            AI.car = this;
            AI.courseController = ctrl;

            CustomInput.IsAI = true;
        }
    }

    //here basically Find() all the relevant items. like wheels, wheel colldiers,
    //instantiate wheel scripts
    private void InitializeCar()
    {
        drivingWheel = gameObject.transform.Find("Steeringwheel");

        //find is not recursive so do this
        Transform samir = gameObject.transform.Find("wheeltransforms");
        wheelFLTrans = samir.transform.Find("FLhub");
        wheelFRTrans = samir.transform.Find("FRhub");
        wheelRLTrans = samir.transform.Find("RLhub");
        wheelRRTrans = samir.transform.Find("RRhub");

        //and again, but with colliders.
        samir = gameObject.transform.Find("wheelcolliders");
        
        wheelFL = samir.transform.Find("FLcollider").GetComponent<WheelCollider>();
        wheelFR = samir.transform.Find("FRcollider").GetComponent<WheelCollider>();
        wheelRL = samir.transform.Find("RLcollider").GetComponent<WheelCollider>();
        wheelRR = samir.transform.Find("RRcollider").GetComponent<WheelCollider>();
        //and also add the tire behavior script
        wheelFL.gameObject.AddComponent<TireBehavior>();
        wheelFR.gameObject.AddComponent<TireBehavior>();
        wheelRL.gameObject.AddComponent<TireBehavior>();
        wheelRR.gameObject.AddComponent<TireBehavior>();
    }

    //move all the FFB gibberish into a method so i can actually see whats going on in Start()
    public void FFBStart()
    {
        // Find an FFB wheel and hook it
        DirectInput directInput = new DirectInput();
        var ret = directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.ForceFeedback);
        if (ret.Count > 0)
        {
            steeringWheel = ret[0];
            steeringWheelJoy = new Joystick(directInput, steeringWheel.InstanceGuid);
            steeringWheelJoy.SetCooperativeLevel(GetWindowHandle(), CooperativeLevel.Exclusive | CooperativeLevel.Background);
            steeringWheelJoy.Properties.AutoCenter = true;


            steeringWheelJoy.Acquire();


            List<int> actuatorsObjectTypes = new List<int>();
            //Get all available force feedback actuators
            foreach (DeviceObjectInstance doi in steeringWheelJoy.GetObjects())
            {
                if ((doi.ObjectId.Flags & DeviceObjectTypeFlags.ForceFeedbackActuator) != 0)
                    actuatorsObjectTypes.Add((int)doi.ObjectId.Flags);
            }

            axes = new Int32[actuatorsObjectTypes.Count];
            int i = 0;
            foreach (int objt in actuatorsObjectTypes)
            {
                axes[i++] = objt;
            }

            //Set effect direction
            dirs = new int[] { 1 };

            ffbForce = new SharpDX.DirectInput.ConstantForce
            {
                Magnitude = 0
            };
            var ep = new EffectParameters
            {
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
            foreach (var effectInfo in allEffects)
            {
                if (effectInfo.Name.Contains("Constant"))
                {
                    constantForceEffect = effectInfo;
                }
            }

            ffbeffect = new Effect(steeringWheelJoy, constantForceEffect.Guid, ep);
            ffbeffect.Start();
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

    public float forceLdebug = 0;
    public float forceRdebug = 0;
    public int forceLCase = 0;
    public int forceRCase = 0;
    private float forceL, forceR;
    private float FFBApproximation(WheelCollider wheelFL, WheelHit wheelHitL, WheelCollider wheelFR, WheelHit wheelHitR) {
        // left side
        if(Mathf.Abs(wheelHitL.sidewaysSlip) < wheelFL.sidewaysFriction.extremumSlip) {
            // from 0 to extremum
            forceL = Mathf.Lerp(0.0f, wheelFL.sidewaysFriction.extremumValue, Mathf.Abs(wheelHitL.sidewaysSlip) / wheelFL.sidewaysFriction.extremumSlip);
            forceLCase = 1;
        } else if(Mathf.Abs(wheelHitL.sidewaysSlip) < wheelFL.sidewaysFriction.asymptoteSlip) {
            // from extremum to asymptote
            forceL = Mathf.Lerp(wheelFL.sidewaysFriction.extremumValue, wheelFL.sidewaysFriction.asymptoteSlip, Mathf.Abs(wheelHitL.sidewaysSlip / wheelFL.sidewaysFriction.extremumSlip));
            forceLCase = 2;
        } else {
            // post asymptote
            forceL = wheelFL.sidewaysFriction.asymptoteValue;
            forceLCase = 3;
        }
        // right side
        if (Mathf.Abs(wheelHitR.sidewaysSlip) < wheelFR.sidewaysFriction.extremumSlip) {
            // from 0 to extremum
            forceR = Mathf.Lerp(0.0f, wheelFR.sidewaysFriction.extremumValue, Mathf.Abs(wheelHitR.sidewaysSlip / wheelFR.sidewaysFriction.extremumSlip));
            forceRCase = 1;
        } else if (Mathf.Abs(wheelHitR.sidewaysSlip) < wheelFR.sidewaysFriction.asymptoteSlip) {
            // from extremum to asymptote
            forceR = Mathf.Lerp(wheelFR.sidewaysFriction.extremumValue, wheelFR.sidewaysFriction.asymptoteSlip, Mathf.Abs(wheelHitR.sidewaysSlip / wheelFR.sidewaysFriction.extremumSlip));
            forceRCase = 2;
        } else {
            // post asymptote
            forceR = wheelFR.sidewaysFriction.asymptoteValue;
            forceRCase = 3;
        }

        // base force

        /*if (forceL > 0 && forceL < 0.1) forceL = 0.1f;
        if (forceL < 0 && forceL > -0.1) forceL = -0.1f;
        if (forceR > 0 && forceR < 0.1) forceR = 0.1f;
        if (forceR < 0 && forceR > -0.1) forceR = -0.1f;*/

        forceLdebug = forceL;
        forceRdebug = forceR;
        return forceL + forceR;
    }

    public int centeringDebug;
    public int ffbMagnitude;
    public float computedFFBDebug, computedFFBDebugRear;

    void FixedUpdate() {
            AeroDrag();

            TireWearMonitor();
            if (manual == 2) clutchPressure = 1 - CustomInput.GetAxis("Clutch");
            Engine();
            float targetSteering = specs.maxSteerAngle * CustomInput.GetAxis("Steering");
        
            // FFB calculations
            if (ffbeffect != null) {

                WheelHit wheelHitR, wheelHitL, wheelHitRR, wheelHitRL;
                wheelFR.GetGroundHit(out wheelHitR);
                wheelFL.GetGroundHit(out wheelHitL);
                wheelRR.GetGroundHit(out wheelHitRR);
                wheelRL.GetGroundHit(out wheelHitRL);

                if (Input.GetButton("logiquake")) {
                    // meme
                    ffbForce.Magnitude = ffbForce.Magnitude < 0 ? 150000 : -150000;
                } else {
                    // non meme
                    // wheel weight calc
                    /*float wheelWeight = 1 - ((((wheelHitR.forwardSlip + wheelHitL.forwardSlip) / 2) + ((wheelHitR.sidewaysSlip + wheelHitL.sidewaysSlip) / 2))/2);
                    float centering = Vector3.SignedAngle(wheelHitR.forwardDir.normalized, GetComponent<Rigidbody>().transform.forward, Vector3.up);
                    float baseForce = (wheelHitR.force + wheelHitL.force) * 5;
                    ffbForce.Magnitude = (int) (baseForce * wheelWeight * centering * -0.5);*/
                    int centering = Mathf.RoundToInt((Vector3.SignedAngle(wheelHitR.forwardDir.normalized, GetComponent<Rigidbody>().transform.forward, Vector3.up) / specs.maxSteerAngle) * -10000); ;
                    centeringDebug = centering;

                    float computedFFBFront = FFBApproximation(wheelFL, wheelHitL, wheelFR, wheelHitR) / FFBMultiplier;
                    float computedFFBRear = FFBApproximation(wheelRL, wheelHitRL, wheelRR, wheelHitRR) / FFBMultiplier;
                    computedFFBDebug = computedFFBFront;
                    computedFFBDebugRear = computedFFBRear;

                    ffbForce.Magnitude = Mathf.RoundToInt(centering * (computedFFBFront));
                    ffbMagnitude = ffbForce.Magnitude;
                }
                EffectParameters ffbParams = new EffectParameters();
                ffbParams.Parameters = ffbForce;
                ffbeffect.SetParameters(ffbParams, EffectParameterFlags.TypeSpecificParameters);
            }

            //steering
            wheelFR.steerAngle = targetSteering;
            wheelFL.steerAngle = targetSteering;

            if(specs.aspirated) Turbo();//turbo sim enable
            
            currentSpeed = 2 * 22 / 7 * wheelFL.radius * ((wheelFL.rpm + wheelFR.rpm) / 2 * specs.frontWheelDriveBias) * 60 / 1000;
            currentSpeed += 2 * 22 / 7 * wheelRL.radius * ((wheelRL.rpm + wheelRR.rpm) / 2 * (1 - specs.frontWheelDriveBias)) * 60 / 1000;
            currentSpeed = Mathf.Round(currentSpeed);
    }

    void Update() {
            Hud.UpdateHUD(engineRPM, specs.engineREDLINE, turboSpool, currentSpeed, shifting, gear, tirewearlist);
            //TODO!!!!!!!!! SOUND SYSTEM RE ENABLE
            Gearbox();//gearbox update
            //EngineAudio.ProcessSounds(engineRPM, spooled, turboSpool);
            LightsOn();
            if (differentialTrans != null) DiffPosition(wheelRRTrans, wheelRLTrans);
            if(drivingWheel != null) drivingWheel.transform.localEulerAngles = new Vector3(drivingWheel.transform.rotation.x, drivingWheel.transform.rotation.y, -90 * CustomInput.GetAxis("Steering"));
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

    void Engine()
    {//engine
        if(gear == 1 || clutchPressure < 0.5f)
        {//NEUTRAL
            if (engineRPM >= specs.engineREDLINE)
            {
                engineRPM -= 300;
            }
            else
            {
                engineRPM += specs.engineTorque.Evaluate(engineRPM) * turboSpool * Mathf.Clamp01(CustomInput.GetAxis("Throttle"));
                if (engineRPM > specs.engineIdle && Mathf.Clamp01(CustomInput.GetAxis("Throttle")) == 0) engineRPM -= specs.engineTorque.Evaluate(engineRPM) / 3;
                if (engineRPM < specs.engineIdle) engineRPM += 20;
            }
            clutchRPM = engineRPM;
            wheelFL.motorTorque = 0;
            wheelFR.motorTorque = 0;
            wheelRL.motorTorque = 0;
            wheelRR.motorTorque = 0;
        }
        else
        {//DRIVE
            engineRPM = (Mathf.Abs(wheelFL.rpm + wheelFR.rpm) / 2 * specs.frontWheelDriveBias) * specs.ratio * Mathf.Abs(specs.gears[gear]);
            engineRPM += (Mathf.Abs(wheelRL.rpm + wheelRR.rpm) / 2 * (1 - specs.frontWheelDriveBias)) * specs.ratio * Mathf.Abs(specs.gears[gear]);
            //make motor torks calc shorter with temporary variable
            float precalc = Mathf.Clamp01(CustomInput.GetAxis("Throttle")) * ((-0.5f + Mathf.Clamp01(specs.gears[gear])) * 2) * clutchPressure * turboSpool;

            engineOUT = specs.engineTorque.Evaluate(engineRPM) * precalc;

            wheelFL.motorTorque = (specs.engineTorque.Evaluate(engineRPM) - Differential(wheelFR, wheelFL)) * CenterDifferential(1) * specs.frontWheelDriveBias * precalc;
            wheelFR.motorTorque = (specs.engineTorque.Evaluate(engineRPM) - Differential(wheelFL, wheelFR)) * CenterDifferential(1) * specs.frontWheelDriveBias * precalc;
            wheelRL.motorTorque = (specs.engineTorque.Evaluate(engineRPM) - Differential(wheelRR, wheelRL)) * CenterDifferential(-1) * (1 - specs.frontWheelDriveBias) * precalc;
            wheelRR.motorTorque = (specs.engineTorque.Evaluate(engineRPM) - Differential(wheelRL, wheelRR)) * CenterDifferential(-1) * (1 - specs.frontWheelDriveBias) * precalc;

            if (engineRPM >= specs.engineREDLINE)
            {//rev limiter
                wheelFL.motorTorque = 0;
                wheelFR.motorTorque = 0;
                wheelRL.motorTorque = 0;
                wheelRR.motorTorque = 0;
            }
            if (engineRPM < specs.engineIdle)
            {//idling
                wheelFL.motorTorque = 100 * specs.frontWheelDriveBias * ((-0.5f + Mathf.Clamp01(specs.gears[gear])) * 2) * clutchPressure;
                wheelFR.motorTorque = 100 * specs.frontWheelDriveBias * ((-0.5f + Mathf.Clamp01(specs.gears[gear])) * 2) * clutchPressure;
                wheelRL.motorTorque = 100 * (1 - specs.frontWheelDriveBias) * ((-0.5f + Mathf.Clamp01(specs.gears[gear])) * 2) * clutchPressure;
                wheelRR.motorTorque = 100 * (1 - specs.frontWheelDriveBias) * ((-0.5f + Mathf.Clamp01(specs.gears[gear])) * 2) * clutchPressure;
            }
            if (clutchRPM > engineRPM)
            {//clutch kick
                clutchRPM -= specs.engineTorque.Evaluate(engineRPM);
                engineRPM = clutchRPM;
                wheelFL.motorTorque = 300 * specs.frontWheelDriveBias * ((-0.5f + Mathf.Clamp01(specs.gears[gear])) * 2) * clutchPressure;
                wheelFR.motorTorque = 300 * specs.frontWheelDriveBias * ((-0.5f + Mathf.Clamp01(specs.gears[gear])) * 2) * clutchPressure;
                wheelRL.motorTorque = 300 * (1 - specs.frontWheelDriveBias) * ((-0.5f + Mathf.Clamp01(specs.gears[gear])) * 2) * clutchPressure;
                wheelRR.motorTorque = 300 * (1 - specs.frontWheelDriveBias) * ((-0.5f + Mathf.Clamp01(specs.gears[gear])) * 2) * clutchPressure;
            }
            else if (clutchRPM + 300 < engineRPM)
            {
                clutchRPM = 0;
            }

        }
        wheelRPM = (wheelFL.rpm * 3.3f) * specs.ratio; //speed counter
    }
    
    //i know its a bit janky but eh whatever
    void TireWearMonitor()
    {
        tirewearlist[0] = wheelFL.gameObject.GetComponent<TireBehavior>().TreadHealth;
        tirewearlist[1] = wheelFR.gameObject.GetComponent<TireBehavior>().TreadHealth;
        tirewearlist[2] = wheelRL.gameObject.GetComponent<TireBehavior>().TreadHealth;
        tirewearlist[3] = wheelRR.gameObject.GetComponent<TireBehavior>().TreadHealth;
    }

    void Turbo()
    {
        if (CustomInput.GetAxis("Throttle") > 0 && engineRPM > specs.engineIdle + 50)
        {
            turboSpool += specs.turboWaste.Evaluate(engineRPM) / (specs.turboSize * 3);
            if (turboSpool > specs.turboWaste.Evaluate(engineRPM))
            {
                turboSpool = specs.turboWaste.Evaluate(engineRPM);
                spooled = true;
            }
        }
        else
        {
            if (turboSpool > 0.2f)
            {
                spooled = false;
                turboSpool -= turboSpool / (specs.turboSize * 4);
            }
        }
    }

    public float CenterDifferential (float front)
    {
        //if car is not awd, dont calculate center diff (DUH!)
        if (specs.frontWheelDriveBias == 0 || specs.frontWheelDriveBias == 1)
        {
            return 1;
        }
        else
        {
            //deduct 50% of power if wheels are faster than fronts
            if (front * ((wheelFL.rpm + wheelFR.rpm) / 2) - ((wheelRL.rpm + wheelRR.rpm) / 2) > specs.lsd)
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
                    if(engineRPM > specs.engineIdle + 900) gear++;
                    break;
                }
                if(engineRPM > specs.engineREDLINE - 500 && gear != specs.gears.Length - 1)
                {
                    //upshift
                    gear++;
                    break;
                }
                if (engineRPM < specs.engineREDLINE - specs.engineREDLINE / 2 && gear != 2 && gear != 1)
                {
                    //downshift
                    gear--;
                    break;
                }
                if (engineRPM < specs.engineIdle + 200 && gear == 2)
                {
                    //downshift to neutral
                    gear--;
                    break;
                }
                break;
            case 1: //MANUAL
                if (Input.GetButtonDown("ShiftUp") == true && gear < specs.gears.Length - 1 && shifting == false)
                {
                    gear = gear + 1;
                    StartCoroutine(ClutchDelay(specs.shifterDelay));
                }
                if (Input.GetButtonDown("ShiftDown") == true && gear > 0 && shifting == false)
                {
                    gear = gear - 1;
                    StartCoroutine(ClutchDelay(specs.shifterDelay / 3));
                }
                break;
            case 2: //MANUAL CLUTCH
                if (Input.GetButtonDown("ShiftUp") == true && gear < specs.gears.Length - 1 && clutchPressure < 0.5f)
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
        airSpeed = Mathf.Abs(gameObject.GetComponent<Rigidbody>().velocity.x) +
            Mathf.Abs(gameObject.GetComponent<Rigidbody>().velocity.y) +
            Mathf.Abs(gameObject.GetComponent<Rigidbody>().velocity.z);

        Vector3 drag = gameObject.transform.forward.normalized * -1;
        drag.y = -specs.aero * airSpeed;//downforce
        
        
        //Debug.Log(drag);
        gameObject.GetComponent<Rigidbody>().angularDrag = Mathf.Lerp(0, 0.8f, airSpeed / 100);
        gameObject.GetComponent<Rigidbody>().drag = Mathf.Lerp(0, specs.dragCoef + specs.aero / 200, airSpeed / 100);
        //Debug.Log(transform.forward.y);
        gameObject.GetComponent<Rigidbody>().AddRelativeForce(drag, ForceMode.Force);

    }
    
    void DiffPosition(Transform rWheel, Transform lWheel)
    {
        Vector3 pos = new Vector3(0, (rWheel.localPosition.y + lWheel.localPosition.y) / 2 - 0.05f, rWheel.localPosition.z);
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
    
    void LightsOn()
    {
        //make this check if(input.getaxis) whatever. TODO.
        if (gear == 595)
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
}
