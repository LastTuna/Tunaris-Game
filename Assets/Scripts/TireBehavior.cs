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
    public float currentGrip;
    float stiffnessF, stiffnessS;//container for grip stiffness
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
    public float fwdSlip;
    public float sidewaySlip;


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
        stiffnessF = tyre.forwardFriction.stiffness;
        stiffnessS = tyre.forwardFriction.stiffness;
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
         WheelHit wheelhit;

        //manage surfaces, and grip.
        // Ground surface detection
        if (tyre.GetGroundHit(out wheelhit) && !burst)
        {
            if (lastSurface != wheelhit.collider.gameObject.tag) { 
                lastSurface = wheelhit.collider.gameObject.tag;
            switch (lastSurface)
            {
                case "sand":
                    //gravel/offroad
                    currentGrip = (1 - treadType) - Dampness() - ((100 - TreadHealth) / 5000);
                    if (dirtiness < 1) dirtiness += Mathf.Abs(tyre.rpm) / 500000;
                    
                    if(tyre.rpm > 600) tyre.suspensionDistance = Random.value / 10;

                    break;
                case "tarmac":
                    //TARMAC/puddle
                    currentGrip = treadType - Dampness() - ((100 - TreadHealth) / 5000);
                    tyre.suspensionDistance = defaultSusp;
                    break;
                case "grass":
                    //grass
                    currentGrip = (treadType / 2) - Dampness() - ((100 - TreadHealth) / 5000);
                    break;
                case "puddle":
                    currentGrip = 0.1f;
                    break;
            }
            if (currentGrip <= 0f)
            {//if grip goes below 0, give wheels minimum 0.13 grip.
                currentGrip = 0.2f;
            }
            else if(Dampness() != 1)
            {//grip preload, to apply some extra grip. do not apply extra grip if in puddle
                currentGrip += 0.2f;
            }
            //move SetStiffness here when done
        }
        }
        if (burst)
        {
            currentGrip = 0.1f;
        }
        //DEBUGGING PURPOSES APPLY GRIP EVERY TICK FOR NOW UNTIL I COMPLETE
        tyre.forwardFriction = SetStiffness(tyre.forwardFriction, stiffnessF);
        tyre.sidewaysFriction = SetStiffness(tyre.sidewaysFriction, stiffnessS);
    }

    public float Dampness()
    {//returns ground dampness value. if in a puddle, return 1.
        WheelHit wheelhit;
        tyre.GetGroundHit(out wheelhit);
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

    WheelFrictionCurve SetStiffness(WheelFrictionCurve wheel, float newStiffness)
    {
        //sets tire grip
        return new WheelFrictionCurve()
        {
            extremumSlip = wheel.extremumSlip,
            extremumValue = wheel.extremumValue,
            asymptoteSlip = wheel.asymptoteSlip,
            asymptoteValue = wheel.asymptoteValue,
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