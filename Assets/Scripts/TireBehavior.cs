using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TireBehavior : MonoBehaviour
{

    public WheelCollider tyre;
    public float tyreType;
    public float TreadHealth = 100;
    public float diameter;
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

        if (Mathf.Abs(GroundHit.sidewaysSlip) > .6)
        {
            TreadHealth = TreadHealth - GroundHit.sidewaysSlip / 10;
        }
    }
    public void GripManager()
    {
         WheelHit FLhit;

        //manage surfaces, and grip.
        // Ground surface detection
        if (tyre.GetGroundHit(out FLhit))
         {
             if (FLhit.collider.gameObject.CompareTag("sand"))
             {
             
             }
             //something for dampness adjust
             //something for puddles
             //something for tarmac
             //something for grass
         }

         



        tyre.sidewaysFriction = SetStiffness(tyre.sidewaysFriction, 1);
        // Rear wheels
        tyre.sidewaysFriction = SetStiffness(tyre.sidewaysFriction, 1);
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