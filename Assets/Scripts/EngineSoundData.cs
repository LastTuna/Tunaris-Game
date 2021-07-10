using System.IO;
using UnityEngine;


//sound system structure
//sample < sample layer < sample index.

[System.Serializable]
public class EngineSample
{
    public int lowRPM = 0;//rpm at which sample starts to play
    public int hiRPM = 830;//end of sample
    public AudioClip clip;//used during runtime, irrelevant in json
    public string clipname = "idle.wav";
    public AnimationCurve audioVolume;//CURVES ARE CLAMPED 0-1
    public AnimationCurve audioPitch;//CURVES ARE CLAMPED 0-1
                                     // Maybe find a way to replace that with a way to parametrize the formula
}
[System.Serializable]
public class EngineSampleIndex
{
    public EngineSample[] SampleLayer = new EngineSample[2];//samples' layers for this index
    public int indexThreshold = 830;//the end of this sample range. after this, samples move on to the next index.
}
[System.Serializable]
public class EngineSoundData
{
    public EngineSampleIndex[] SampleIndex = new EngineSampleIndex[2];//sample indexes
    public string[] pshhNames = new string[] { "wastegate1.wav" };//wastegate samples. can have several different. it will pick at random.
    public string boostName = "whee.wav";//turbo whine name
    public string hornName = "doot.wav";//doot doot
    public AudioClip horn;//used during runtime
    public AudioClip[] pshh;
    public AudioClip boost;
    public string SoundbankInfo = "sample engine sound bank";//metadata



    //debug func. used to export tires that you made during runtime.
    public void ExportData(EngineSoundData AAASSSSS)
    {
        //export a json to EXPORT folder. has default values.
        //trigger this by whatever you want really. not used ingame.
        string filePath = Application.dataPath + "/EXPORT/" + "soundbank.json";
        string dataAsJson = JsonUtility.ToJson(AAASSSSS, true);
        File.WriteAllText(filePath, dataAsJson);
        Debug.Log("soundbank EXPORTED TO " + filePath);
    }

    public EngineSoundData ImportData(string dataAsJson)
    {
        EngineSoundData dolor;
        dolor = JsonUtility.FromJson<EngineSoundData>(dataAsJson);
        return dolor;
    }
}

