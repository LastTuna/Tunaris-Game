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
        if (!beer) return;
        if (import)
        {
            import = false;
            treadGripforward = new AnimationCurve();
            treadGripsideways = new AnimationCurve();
            
            treadGripforward.AddKey(beer.forwardFriction.extremumSlip, beer.forwardFriction.extremumValue);
            treadGripforward.AddKey(beer.forwardFriction.asymptoteSlip, beer.forwardFriction.asymptoteValue);

            treadGripsideways.AddKey(beer.sidewaysFriction.extremumSlip, beer.sidewaysFriction.extremumValue);
            treadGripsideways.AddKey(beer.sidewaysFriction.asymptoteSlip, beer.sidewaysFriction.asymptoteValue);
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
            extremumSlip = Mathf.Round(jamal.keys[0].time * 100) / 100,
            extremumValue = Mathf.Round(jamal.keys[0].value * 100) / 100,
            asymptoteSlip = Mathf.Round(jamal.keys[1].time * 100) / 100,
            asymptoteValue = Mathf.Round(jamal.keys[1].value * 100) / 100,
            stiffness = 1
        };
    }
}
