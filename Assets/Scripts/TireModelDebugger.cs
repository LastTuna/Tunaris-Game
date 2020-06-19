using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TireModelDebugger : MonoBehaviour {

    public WheelCollider beer;
    public bool import;
    public bool export;
    public AnimationCurve treadGripforward = new AnimationCurve();
    public AnimationCurve treadGripsideways = new AnimationCurve();
    public float currentSlip;


    // Use this for initialization
    void Start () {
        beer = gameObject.GetComponent<WheelCollider>();


	}

    void FixedUpdate()
    {
        if (import)
        {
            import = false;
            treadGripforward = new AnimationCurve();
            treadGripsideways = new AnimationCurve();
            
            treadGripforward.AddKey(beer.forwardFriction.extremumValue, beer.forwardFriction.extremumSlip);
            treadGripforward.AddKey(beer.forwardFriction.asymptoteValue, beer.forwardFriction.asymptoteSlip);

            treadGripsideways.AddKey(beer.sidewaysFriction.extremumValue, beer.sidewaysFriction.extremumSlip);
            treadGripsideways.AddKey(beer.sidewaysFriction.asymptoteValue, beer.sidewaysFriction.asymptoteSlip);
            Debug.Log("tread curve import ok");
        }

        if (export)
        {
            export = false;

            beer.forwardFriction = SetStiffness(beer.forwardFriction, treadGripforward);
            beer.sidewaysFriction = SetStiffness(beer.sidewaysFriction, treadGripsideways);
            Debug.Log("exported");
        }
        WheelHit despacito;
        beer.GetGroundHit(out despacito);
        currentSlip = Mathf.Abs(despacito.sidewaysSlip) + Mathf.Abs(despacito.forwardSlip);

    }


    WheelFrictionCurve SetStiffness(WheelFrictionCurve wheel, AnimationCurve jamal)
    {
        return new WheelFrictionCurve()
        {
            extremumSlip = Mathf.Round(jamal.keys[0].value * 10) / 10,
            extremumValue = Mathf.Round(jamal.keys[0].time * 10) / 10,
            asymptoteSlip = Mathf.Round(jamal.keys[1].value * 10) / 10,
            asymptoteValue = Mathf.Round(jamal.keys[1].time * 10) / 10,
            stiffness = 1
        };
    }
}
