using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TireBehavior : MonoBehaviour
{

    public WheelCollider tyre;
    public float treadType;
    public float TreadHealth = 100;
    public float brakeStrength = 300;
    public bool handbrake = false;
    public float brakeHeat = 26;//celsius
    public float diameter;
    public float groundDampness;//get this value from to-be-implemented weather controller.
    public float currentGrip;
    //when it rains, this controls how damp the ground is/effects grip.
    public bool burst = false;//feature to be added? tire wear causes more prone to bursting
    public Material brakeMat;
    public Material dirt; //dirt MATERIAL.
    public Renderer disc;
    public Renderer dirtMesh; //fetches and instantiates dirt material
    public float dirtiness;
    public float defaultSusp;
    public AnimationCurve brakeFadeCurve = new AnimationCurve(
        new Keyframe(0, 0.4f),
        new Keyframe(420, 1f),
        new Keyframe(600, 1f),
        new Keyframe(800, 0.3f)
        );
    public float wheelrpm;
    // Use this for initialization
    void Start()
    {
        brakeMat = disc.GetComponent<Renderer>().materials[1];
        dirt = dirtMesh.GetComponent<Renderer>().material;
        treadType = FindObjectOfType<DataController>().TireBias;
        diameter = tyre.radius;
        defaultSusp = tyre.suspensionDistance;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        TyreWear();
        GripManager();
        Brakes();
    }
    void Update()
    {
        dirt.color = new Color(1, 1, 1, dirtiness);
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

    public void GripManager()
    {
         WheelHit wheelhit;

        //manage surfaces, and grip.
        // Ground surface detection
        if (tyre.GetGroundHit(out wheelhit) && !burst)
        {
            string despacito = wheelhit.collider.gameObject.tag;
            wheelrpm = tyre.rpm;
            switch (despacito)
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
        }
        if (burst)
        {
            currentGrip = 0.1f;
        }
        tyre.forwardFriction = SetStiffness(tyre.sidewaysFriction, currentGrip);
        // Rear wheels
        tyre.sidewaysFriction = SetStiffness(tyre.sidewaysFriction, currentGrip);
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
                brakeHeat += Mathf.Abs(tyre.rpm) / 50;
            }
            tyre.brakeTorque = brakeStrength * brakeFadeCurve.Evaluate(brakeHeat);
        }
        else
        {
            tyre.brakeTorque = 0;
        }
        if (Input.GetAxis("Handbrake") > 0f && handbrake)//HANDBRAKE
        {
            tyre.brakeTorque = 700;
        }
        if (brakeHeat > 50)
            brakeHeat = brakeHeat - brakeHeat / 500;
        if (brakeHeat > 400)
            brakeMat.SetColor("_EmissionColor", new Color(1 - brakeFadeCurve.Evaluate(brakeHeat), (1 - brakeFadeCurve.Evaluate(brakeHeat)) / 2, 0));
    }

    WheelFrictionCurve SetStiffness(WheelFrictionCurve current, float newStiffness)
    {
        //sets tire grip
        return new WheelFrictionCurve()
        {
            
            asymptoteSlip = tyre.sidewaysFriction.asymptoteSlip,
            asymptoteValue = tyre.sidewaysFriction.asymptoteValue,
            extremumSlip = tyre.sidewaysFriction.extremumSlip,
            extremumValue = tyre.sidewaysFriction.extremumValue,
            stiffness = newStiffness
        };
    }



}