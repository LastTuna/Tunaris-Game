using System.IO;
using UnityEngine;

public class CarData {
    
    //general
    public string description = "this is temp car";
    //car description
    public string version = "0.0.6";
    //just a version number
    public float weight = 100;
    //cor mass. dont forget wheels also have mass.
    public Vector3 centerOfGravy = new Vector3(0, 0.2f, 0);
    //cog setup. unity fucks about with this, so depite placing it where you want it, the position is arbitrary 

    //aero
    public float aero = 5f;
    //aero push down force
    public float dragCoef = 0.06f;
    //max rigidbody.drag value.-drag reaches this value at some 150mph or sth
    //this also affects stability in high speeds. higher value = more stable

    //engine
    public float engineIdle = 800;
    //idle rpm
    public float engineREDLINE = 9000;
    //engine redline
    public AnimationCurve engineTorque = new AnimationCurve(new Keyframe(0, 130), new Keyframe(5000, 250), new Keyframe(9000, 200));
    //engine power

    //diff & tranny
    public float frontWheelDriveBias = 0.5f;
    //0 - rear, 1 - front - center diff
    public float ratio = 4.3f;
    //final drive
    public float[] gears = new float[8] { -5.0f, 0.0f, 5.4f, 3.4f, 2.7f, 2.0f, 1.8f, 1.6f };
    //gears
    public float shifterDelay = 0.3f;
    //auto and manual shifter delay time
    public float maxSteerAngle = 20;
    //steering lock
    public float lsd = 1f;
    //an arbitrary threshold between wheels on an axle. 0 = open diff

    //turbo (SNEED)
    public bool aspirated = false;
    //flag to enable turbo sim
    public AnimationCurve turboWaste = new AnimationCurve(new Keyframe(0, 0), new Keyframe(5000, 1), new Keyframe(9000, 0));
    //adjust turbo engagement curve, turbo pressure
    public float turboSize = 10;
    //mm - adjusts turbo lag


        //feed a json in string to this and it returns it as CarData.
        //used in unpacking data in CarBehaviour.cs
    public CarData ImportData(string dataAsJson)
    {
        CarData dolor;
        dolor = JsonUtility.FromJson<CarData>(dataAsJson);
        return dolor;
    }
    
}
