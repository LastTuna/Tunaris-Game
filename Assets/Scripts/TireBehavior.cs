using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TireBehavior : MonoBehaviour
{

    public WheelCollider tyre;
    public GameObject visualWheel;
    public Transform wheelTransform, caliper;


    public float treadType;
    public float TreadHealth = 100;
    public float brakeStrength = 300;
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
    public float slipTreshold = 0.2f;
    public TreadBehavior[] tyreTread;
    //multidim array
    //first index: tarmac
    //second index: gravel
    //third index: grass
    //fourth: snow (future support)
    private float tireTicker = 0;//call wheel tread update every x ticks

    public float wheelrpm;
    // Use this for initialization
    void Start()
    {
        dirt = visualWheel.GetComponent<Renderer>().materials[0];
        brakeMat = visualWheel.GetComponent<Renderer>().materials[1];
        treadType = FindObjectOfType<DataController>().TireBias;
        brakeStrength = FindObjectOfType<DataController>().BrakeStiffness;
        diameter = tyre.radius;
        defaultSusp = tyre.suspensionDistance;
        smokeEmitter = Instantiate(smokeEmitter, gameObject.transform);//debug for now...

        float springStiffness = FindObjectOfType<DataController>().SpringStiffness;
        springs.spring = springStiffness;
        springs.damper = springStiffness / 10;

        tyre.suspensionSpring = springs;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        TyreWear();
        GripManager();
        Brakes();
        wheelrpm = tyre.rpm;
        SmonkEmitter();
    }
    void Update()
    {
        WheelPosition();
    }

    public void TyreWear()
    {
        WheelHit GroundHit;
        tyre.GetGroundHit(out GroundHit);
        if (!burst && TreadHealth > 0)
        {
            TreadHealth = TreadHealth - (Mathf.Abs(GroundHit.sidewaysSlip) + Mathf.Abs(GroundHit.forwardSlip)) / 200;
            tyre.radius = diameter;
        }
        else
        {
            tyre.radius = diameter - 0.1f;
            burst = true;
        }
    }

    public void SmonkEmitter()
    {
        WheelHit wheelhit;
        tyre.GetGroundHit(out wheelhit);
        float slip = Mathf.Abs(wheelhit.forwardSlip) + Mathf.Abs(wheelhit.sidewaysSlip);
        
        if (slip > slipTreshold)
        {
            if (!smokeEmitter.GetComponent<ParticleSystem>().isPlaying)
            {
                smokeEmitter.GetComponent<ParticleSystem>().Play();
            }
        }
        else
        {
            smokeEmitter.GetComponent<ParticleSystem>().Stop();
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
            stiffness = 1 - Dampness(wheelhit) - ((100 - TreadHealth) / 1000);
            lastSurface = wheelhit.collider.gameObject.tag;
            if (burst)
            {
                stiffness = 0.1f;
            }
            tyre.forwardFriction = SetStiffness(tyreTread[surface].forwardCurve, stiffness);
            tyre.sidewaysFriction = SetStiffness(tyreTread[surface].sidewaysCurve, stiffness);
            tireTicker = 0;
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

    WheelFrictionCurve SetStiffness(float[] wheel, float newStiffness)
    {
        //sets tire grip
        return new WheelFrictionCurve()
        {
            extremumSlip = wheel[0],
            extremumValue = wheel[1],
            asymptoteSlip = wheel[2],
            asymptoteValue = wheel[3],
            stiffness = newStiffness
        };
    }

    public JointSpring springs = new JointSpring
    {
        //should load data from data controller when setup
        spring = 8000,
        damper = 800,
        targetPosition = 0.5f,
    };

    void WheelPosition()
    {
        RaycastHit hit;
        Vector3 wheelPos;
        if (Physics.Raycast(tyre.transform.position, -tyre.transform.up, out hit, tyre.radius + tyre.suspensionDistance))
        {
            wheelPos = hit.point + tyre.transform.up * tyre.radius;
            //UPDATE DIRTINESS
            dirt.SetFloat("_FortniteRange", dirtiness);
        }
        else
        {
            wheelPos = tyre.transform.position - tyre.transform.up * tyre.suspensionDistance;
        }
        caliper.position = wheelPos;
        wheelTransform.position = wheelPos;

        //also do the spinny things
        wheelTransform.Rotate(tyre.rpm / 60 * 360 * Time.deltaTime, 0, 0);
        wheelTransform.localEulerAngles = new Vector3(wheelTransform.localEulerAngles.x, tyre.steerAngle - wheelTransform.localEulerAngles.z, wheelTransform.localEulerAngles.z);
        caliper.localEulerAngles = new Vector3(caliper.localEulerAngles.x, tyre.steerAngle - caliper.localEulerAngles.z, caliper.localEulerAngles.z);

    }

}

[System.Serializable]
public class TreadBehavior
{
    public float[] forwardCurve;
    public float[] sidewaysCurve;
}