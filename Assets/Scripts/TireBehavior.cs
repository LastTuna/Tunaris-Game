using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TireBehavior : MonoBehaviour
{

    public WheelCollider tyre;
    public float tyreType;
    public float TreadHealth = 100;
    public float diameter;
    public float groundDampness;//get this value from to-be-implemented weather controller.
    public float currentGrip;
    //when it rains, this controls how damp the ground is/effects grip.
    public bool burst = false;//feature to be added? tire wear causes more prone to bursting

    // Use this for initialization
    void Start()
    {
        tyreType = FindObjectOfType<DataController>().TireBias;
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
        
            TreadHealth = TreadHealth - (Mathf.Abs(GroundHit.sidewaysSlip) + Mathf.Abs(GroundHit.forwardSlip)) / 200;
    }
    public void GripManager()
    {
         WheelHit wheelhit;

        //manage surfaces, and grip.
        // Ground surface detection
        if (tyre.GetGroundHit(out wheelhit))
        {
            if (wheelhit.collider.gameObject.CompareTag("sand"))
            {//gravel/offroad
                currentGrip = (1 - tyreType) - Dampness() - ((100 - TreadHealth) / 5000);
            }
            if (wheelhit.collider.gameObject.CompareTag("tarmac") || wheelhit.collider.gameObject.CompareTag("puddle"))
            {//TARMAC/puddle
                currentGrip = tyreType - Dampness() - ((100 - TreadHealth) / 5000);
            }
            if (wheelhit.collider.gameObject.CompareTag("grass"))
            {//grass
                currentGrip = (tyreType / 2) - Dampness() - ((100 - TreadHealth) / 5000);
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
        tyre.forwardFriction = SetStiffness(tyre.sidewaysFriction, currentGrip);
        // Rear wheels
        tyre.sidewaysFriction = SetStiffness(tyre.sidewaysFriction, currentGrip);
    }
    public float Dampness()
    {
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