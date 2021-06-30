using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TireBehavior : MonoBehaviour
{

    public WheelCollider tyre;
    public GameObject visualWheel;
    public Transform wheelHub, suspension;
    
    public int TireIndex = 0;//the number of the compound in TireData
    public float TreadHealth = 100;
    public float brakeStrength = 100;
    public bool handbrake = false;
    public bool raceStartHandbrake = false;
    public float brakeHeat = 26;//celsius
    public float diameter;
    public float groundDampness;//get this value from to-be-implemented weather controller.
    //when it rains, this controls how damp the ground is/effects grip.
    public bool burst = false;//feature to be added? tire wear causes more prone to bursting
    Material brakeMat;
    Material dirt; //dirt MATERIAL.
    public float dirtiness;
    public float defaultSusp;
    public AnimationCurve brakeFadeCurve = new AnimationCurve(
        new Keyframe(0, 0.4f),
        new Keyframe(420, 1f),
        new Keyframe(600, 1f),
        new Keyframe(800, 0.3f)
        );
    public string lastSurface;

    public GameObject smokeEmitter;//debugging thing
    public float forwardSlipTreshold;
    public float sidewaysSlipThreshold;
    public float forwardSlip;
    public float sidewaysSlip;
    public CompoundData tireCompound;
    private float tireTicker = 0;//call wheel tread update every x ticks
    public float wheelrpm;
    // Use this for initialization
    void Start()
    {
        //do this then do whatever else
        InitializeTire();

        //also add here the part where you fetch the tire data from
        //the tire json in the assetbundle. use content manager.

        handbrake = tireCompound.handbrake;
        //initiate some reference values.
        diameter = tyre.radius;
        defaultSusp = tyre.suspensionDistance;
        tyre.suspensionSpring = springs;

        if (!visualWheel) {
            Debug.LogError("TireBehavior::Start no visualWheel");
        } else {
            dirt = visualWheel.GetComponent<Renderer>().materials[0];
            brakeMat = visualWheel.GetComponent<Renderer>().materials[1];
        }
        if (!smokeEmitter) {
            Debug.LogError("TireBehavior::Start no smokeEmitter");
        } else {
            smokeEmitter = Instantiate(smokeEmitter, gameObject.transform);//debug for now...
            smokeEmitter.GetComponent<ParticleSystem>().Play();
        }
    }

    //here basically Find() all the relevant items. like wheel, wheel colldier
    private void InitializeTire()
    {
        tyre = gameObject.GetComponent<WheelCollider>();
        GameObject basecar = gameObject.transform.parent.parent.gameObject;
        basecar = basecar.transform.Find("wheeltransforms").gameObject;
        //get the current wheel identity (FR,FL,RR,RL you get it)
        string wheelIdentity = gameObject.name.Substring(0, 2);
        wheelHub = basecar.transform.Find(wheelIdentity + "hub");
        visualWheel = wheelHub.Find(wheelIdentity + "wheel").gameObject;
        suspension = basecar.transform.Find(wheelIdentity + "susp");

        //write here code to get the tire compound json
        //meanwhile just initiate tires with defaults.
        TireData tires = new TireData();
        if (wheelIdentity == "FR" || wheelIdentity == "FL")
        {
            Debug.Log(tires.tireCompounds[0].FrontTires.handbrake);
            tireCompound = tires.tireCompounds[TireIndex].FrontTires;
            Debug.Log(tireCompound.handbrake);
        }
        else
        {
            tireCompound = tires.tireCompounds[TireIndex].RearTires;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        TyreWear();
        GripManager();
        Brakes();
        wheelrpm = tyre.rpm;
        if (smokeEmitter) {
            SmonkEmitter();
        }

    }

    void Update()
    {
        WheelPosition();
    }

    public void TyreWear()
    {
        WheelHit GroundHit;
        if (!burst && TreadHealth > 0 && tyre.GetGroundHit(out GroundHit))
        {
            TreadHealth = TreadHealth - (Mathf.Abs(GroundHit.sidewaysSlip) + Mathf.Abs(GroundHit.forwardSlip)) / tireCompound.wearFactor;
            tyre.radius = diameter;
        }
        else
        {
            tyre.radius = diameter - 0.1f;
            burst = true;
        }
    }
    
    public bool smokeState = false;
    public void SmonkEmitter() {
        WheelHit wheelhit;
        tyre.GetGroundHit(out wheelhit);

        forwardSlip = Mathf.Abs(wheelhit.forwardSlip);
        sidewaysSlip = Mathf.Abs(wheelhit.sidewaysSlip);

        if (forwardSlip + sidewaysSlip > forwardSlipTreshold + sidewaysSlipThreshold) {
            ParticleSystem.EmissionModule em = smokeEmitter.GetComponent<ParticleSystem>().emission;
            em.enabled = true;
            smokeState = true;
        } else {
            ParticleSystem.EmissionModule em = smokeEmitter.GetComponent<ParticleSystem>().emission;
            em.enabled = false;
            smokeState = false;
        }
    }

    public void GripManager()
    {
        int surface = 0;//0road,1gravel,2grass,3snow
        float stiffness = 0.1f;
        switch (lastSurface)
        {
            case "tarmac":
                //TARMAC
                surface = 0;
                tyre.suspensionDistance = defaultSusp;
                break;

            case "sand":
                //gravel/offroad
                surface = 1;
                if (dirtiness < 1) dirtiness += Mathf.Abs(tyre.rpm) / 500000;
                if (tyre.rpm > 600) tyre.suspensionDistance = Random.value / 10;
                break;

            case "grass":
                //grass
                surface = 2;
                break;

            case "snow":
                surface = 3;
                break;

            case "puddle":
                stiffness = 0.1f;
                break;
        }

        //call tire physics update every 4 or so physics ticks
        //make sure car touches ground ok
        WheelHit wheelhit;
        if (tyre.GetGroundHit(out wheelhit) && tireTicker >= 4)
        {
            stiffness = tireCompound.ForwardFric[surface].stiffness - Dampness(wheelhit) - ((100 - TreadHealth) / 1000);
            lastSurface = wheelhit.collider.gameObject.tag;
            if (burst)
            {
                stiffness = 0.1f;
            }
            tyre.forwardFriction = SetStiffness(tireCompound.ForwardFric[surface]);
            tyre.sidewaysFriction = SetStiffness(tireCompound.SidewaysFric[surface]);
            tireTicker = 0;

            // update smoke emitter thresholds based on new tire parameters
            // idk i feel like adding 2% after max grip before it skids sounds cool
            forwardSlipTreshold = tyre.forwardFriction.extremumSlip * 1.02f;
            sidewaysSlipThreshold = tyre.sidewaysFriction.extremumSlip * 1.02f;
        }
        tireTicker++;
    }

    public float Dampness(WheelHit wheelhit)
    {//returns ground dampness value. if in a puddle, return 1.
        if (wheelhit.collider.gameObject.CompareTag("puddle"))
        {
            return 1f;
        }
            return groundDampness * 0.001f;
    }
    

    void Brakes()
    {
        if (Input.GetAxis("Brake") > 0f)
        {//brakes
            if (brakeHeat < 800)
            {
                brakeHeat += Mathf.Abs(tyre.rpm) / 100 * Input.GetAxis("Brake");
            }
            tyre.brakeTorque = brakeStrength * brakeFadeCurve.Evaluate(brakeHeat) * Input.GetAxis("Brake");
        }
        else
        {
            tyre.brakeTorque = 0;
        }
        if (raceStartHandbrake || (Input.GetAxis("Handbrake") > 0f && handbrake))//HANDBRAKE
        {
            tyre.brakeTorque = 700;
        }

        if (brakeHeat > 50)
            brakeHeat = brakeHeat - brakeHeat / 500;
        if (brakeHeat > 400)
            brakeMat.SetColor("_EmissionColor", new Color(1 - brakeFadeCurve.Evaluate(brakeHeat), (1 - brakeFadeCurve.Evaluate(brakeHeat)) / 2, 0));
    }

    //referenced in the debug UI.
    public float BrakeOutput()
    {
        return brakeStrength * brakeFadeCurve.Evaluate(brakeHeat);
    }

    WheelFrictionCurve SetStiffness(WheelFricCurve fric)
    {
        //sets tire grip
        return new WheelFrictionCurve()
        {
            extremumSlip = fric.extremumSlip,
            extremumValue = fric.extremumValue,
            asymptoteSlip = fric.asymptoteSlip,
            asymptoteValue = fric.asymptoteValue,
            stiffness = fric.stiffness
        };
    }

    public JointSpring springs = new JointSpring
    {
        //should load data from data controller when setup
        spring = 8000,
        damper = 800,
        targetPosition = 0.5f,
    };

    private bool whineAboutCaliper = true;
    void WheelPosition()
    {
        RaycastHit hit;
        Vector3 wheelPos;
        if (Physics.Raycast(tyre.transform.position, -tyre.transform.up, out hit, tyre.radius + tyre.suspensionDistance))
        {
            wheelPos = hit.point + tyre.transform.up * tyre.radius;
            //UPDATE DIRTINESS
            if (dirt) { dirt.SetFloat("_FortniteRange", dirtiness); }
        }
        else
        {
            wheelPos = tyre.transform.position - tyre.transform.up * tyre.suspensionDistance;
        }
        if (!suspension){
            if (whineAboutCaliper) {
                whineAboutCaliper = false;
                Debug.LogError("TireBehavior::WheelPosition no calipers");
            }
        } else {
            suspension.position = wheelPos;
        }
        if (wheelHub) {
            wheelHub.position = wheelPos;

            //also do the spinny things
            wheelHub.Rotate(tyre.rpm / 60 * 360 * Time.deltaTime, 0, 0);
            wheelHub.localEulerAngles = new Vector3(wheelHub.localEulerAngles.x, tyre.steerAngle - wheelHub.localEulerAngles.z, wheelHub.localEulerAngles.z);
        }
        if (suspension) {
            suspension.localEulerAngles = new Vector3(suspension.localEulerAngles.x, tyre.steerAngle - suspension.localEulerAngles.z, suspension.localEulerAngles.z);
        }
    }
}