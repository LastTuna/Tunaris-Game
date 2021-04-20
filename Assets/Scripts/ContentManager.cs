using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;


//this object will be DontDestroyOnLoad(). it contains
//complete car instances
//complete track content
//other custom content.

class ContentManager : MonoBehaviour {
    
    //this is where all assetbundles are stored.
    public List<AssetBundle> Cars;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        //content manager by default exists in main menu scene
        //so making sure when returning to main menu there wont
        //be a second instance.
        //sort something out here becasuse i fuckin cant BRUUUUUH


        GenerateManifest();


        //im just doing some hardcoded thing to make basic functionality again.
        //this should be looped with the data from the manifest to get
        //all the cars into the list. essentially just replace "tempcar"

        AssetBundle corr = GetCarFromFile("tempcar");
        //not sure if i need to unload corr inbetween. leaving as cliffnote
        Cars.Add(corr);

    }

    //this writes the manifest
    public void GenerateManifest()
    {
        Manifest manifest = new Manifest();
        string filePath = Application.dataPath + "/Content/Cars";
        //get all the files in the folder
        string[] penus = Directory.GetFiles(filePath);
        //because we dont want to add other garbage into the manifest,
        //make sure car files end in something. (.sneed)
        foreach (string d in penus)
        {
            if (d.EndsWith(".sneed"))
            {
                manifest.data.Add(Path.GetFileNameWithoutExtension(d));
            }
        }
        string dataAsJson = JsonUtility.ToJson(manifest);
        File.WriteAllText(filePath + "/manifest.json", dataAsJson);
        Debug.Log("manifest.json has been refreshed.");
    }
    
    //this finds the manifest file, unpacks, and return as an array.
    public string[] LoadManifest()
    {
        string filePath = Application.dataPath + "/Content/Cars/manifest.json";
        Manifest penus = JsonUtility.FromJson<Manifest>(File.ReadAllText(filePath));

        //convert the list<string> to an array.
        string[] manifestToArray = new string[penus.data.Count];
        for (int i = 0; i < penus.data.Count; i++)
        {
            manifestToArray[i] = penus.data[i];
        }
        
        return manifestToArray;
    }

    
    
    public AssetBundle GetCarFromFile(string carName)
    {
        string filePath = Application.dataPath + "/Content/Cars/" + carName + ".sneed";
        Debug.Log(filePath);
        AssetBundle corr = AssetBundle.LoadFromFile(filePath);

        return corr;
    }


        //used in car behavior on instantiate
        //used in 1000 other places
    public AssetBundle GetCar(string carName)
    {
        foreach(AssetBundle e in Cars)
        {
            if(e.name == carName)
            {
                return e;
            }
        }
        //failsafe in case car is not found, find tempcar and send it back.
        //tbh maybe make tempcar first car on list? ill have to see about it.
        //hardcoding it in isnt the most retarded idea either.
        foreach (AssetBundle e in Cars)
        {
            if (e.name == "tempcar")
            {
                return e;
            }
        }
        //if you end up here, user deleted tempcar and deserves to have
        //their shit freeze, fuckin wanker
        return null;

    }

    



    //just a debug func
    public void ExportData()
    {
        //export a json to EXPORT folder. has default values.
        //trigger this by whatever you want really. not used ingame.
        CarData penis = new CarData();

        string filePath = Application.dataPath + "/EXPORT/" + "penis.json";
        string dataAsJson = JsonUtility.ToJson(penis, true);
        File.WriteAllText(filePath, dataAsJson);
        Debug.Log("PEN IS MUSIC" + filePath);
    }



}

public class Manifest
{
    public List<string> data = new List<string>();
}