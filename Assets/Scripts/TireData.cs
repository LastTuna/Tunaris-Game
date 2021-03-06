﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


//TIRE DATA - THIS IS THE MAIN CONTAINER FOR ALL THE TIRES INCLUDED WITH THE CAR
[System.Serializable]
public class TireData
{
    public TireSet[] tireCompounds;
    public TireData()
    {
        tireCompounds = new TireSet[] { new TireSet() };
    }

    //debug func. used to export tires that you made during runtime.
    public void ExportData(TireData AAASSSSS)
    {
        //export a json to EXPORT folder. has default values.
        //trigger this by whatever you want really. not used ingame.
        string filePath = Application.dataPath + "/EXPORT/" + "tires.json";
        string dataAsJson = JsonUtility.ToJson(AAASSSSS, true);
        File.WriteAllText(filePath, dataAsJson);
        Debug.Log("TIRE SET EXPORTED TO " + filePath);
    }

    public TireData ImportData(string dataAsJson)
    {
        TireData dolor;
        dolor = JsonUtility.FromJson<TireData>(dataAsJson);
        return dolor;
    }


}

[System.Serializable]
public class TireSet
{
    public CompoundData FrontTires = new CompoundData();
    public CompoundData RearTires = new CompoundData();
}

[System.Serializable]
public class CompoundData
{
    public string treadName = "Grueso Semislick";
    //first index: tarmac
    //second index: gravel
    public WheelFricCurve[] ForwardFric = new WheelFricCurve[] { new WheelFricCurve(), new WheelFricCurve() };
    public WheelFricCurve[] SidewaysFric = new WheelFricCurve[] { new WheelFricCurve(), new WheelFricCurve() };
    public float wearFactor = 200;//bigger number = harder compound
    public bool handbrake = false;//is handbrake enabled on this axle
                                  //brake fade curve
    public AnimationCurve brakeFadeCurve = new AnimationCurve(
    new Keyframe(0, 0.4f),
    new Keyframe(420, 1f),
    new Keyframe(600, 1f),
    new Keyframe(800, 0.3f)
    );
}

//array - 0=front, 1=rear. this is to make access easier in tire behavior.
[System.Serializable]
public class WheelFricCurve
{
    public float extremumSlip = 0.4f;
    public float extremumValue = 1;
    public float asymptoteSlip = 0.8f;
    public float asymptoteValue = 0.5f;
    public float stiffness = 1;

}