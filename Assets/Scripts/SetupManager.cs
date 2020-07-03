using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SetupManager : MonoBehaviour {

    // Use this for initialization











}


[Serializable]
public class SetupData
{
    public string CarName;
    public string SetupDescription;

    public float Aero;
    public float FinalDrive;
    public float LSD;
    public float TorqueSplit;
    public float SteerLock;
    public float FrontSpringStiffness;
    public float RearSpringStiffness;
    public float FrontDamperStiffness;
    public float RearDamperStiffness;
    public float BrakeStrength;
    public float BrakeTorqueSplit;
    public int Gearbox;
}