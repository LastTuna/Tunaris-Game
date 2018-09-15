using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TireBehavior : MonoBehaviour
{

    public WheelCollider tyre;
    public float treadType;
    public float TreadHealth = 100;
    public float diameter;
    public float groundDampness;//get this value from to-be-implemented weather controller.
    public float currentGrip;
    //when it rains, this controls how damp the ground is/effects grip.
    public bool burst = false;//feature to be added? tire wear causes more prone to bursting

    // Use this for initialization
    void Start()
    {
        treadType = FindObjectOfType<DataController>().TireBias;
        diameter = tyre.radius;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        TyreWear();
        GripManager();

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
            if (wheelhit.collider.gameObject.CompareTag("sand"))
            {//gravel/offroad
                currentGrip = (1 - treadType) - Dampness() - ((100 - TreadHealth) / 5000);
            }
            if (wheelhit.collider.gameObject.CompareTag("tarmac") || wheelhit.collider.gameObject.CompareTag("puddle"))
            {//TARMAC/puddle
                currentGrip = treadType - Dampness() - ((100 - TreadHealth) / 5000);
            }
            if (wheelhit.collider.gameObject.CompareTag("grass"))
            {//grass
                currentGrip = (treadType / 2) - Dampness() - ((100 - TreadHealth) / 5000);
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