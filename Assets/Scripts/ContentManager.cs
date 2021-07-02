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

    //this is where all LOADED assetbundles are stored. once UNLOADED, remove from the list.
    public List<AssetBundle> Cars;
    public const string fileExtension = ".tgm";

    //figure out some file extension thats not .sneed in the secret shadow government meeting

    public GameObject DefaultHUD;//this is where i slap default HUD. TROLLFACE

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        //content manager by default exists in main menu scene
        //so making sure when returning to main menu there wont
        //be a second instance.
        //sort something out here becasuse i fuckin cant BRUUUUUH

        GenerateManifest();
        //ExportData();

        //im just doing some hardcoded thing to make basic functionality again.
        //this should be looped with the data from the manifest to get
        //all the cars into the list. essentially just replace "tempcar"


        //this is just make a new tire data object n export a sample json.
        TireData asdadsasd = new TireData();
        asdadsasd.ExportData(asdadsasd);
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
            if (d.EndsWith(fileExtension))
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

    //helper function, for unloading content
    //unsafeUnload = true will unload everything from the bundle, regardless.
    //false will keep whatever current active elements in memory (active gameobjects etc).
    public void UnloadCar(string carName, bool unsafeUnload)
    {
        int i = 0;
        while (true)
        {
            if (Cars[i].name == carName)
            {
                Cars[i].Unload(unsafeUnload);
                //we are completely unloading the asset, so remove from the loaded objects list.
                if (unsafeUnload)
                {
                    Cars.Remove(Cars[i]);
                }
                break;
            }
            i++;
        }
    }
    
    //unloads ALL CONTENT. UNSAFE UNLOAD.
    public void UnloadAllContent()
    {
        while(true)
        {
            if(Cars.Count == 0)
            {
                break;
            }
            Cars[0].Unload(true);
            Cars.Remove(Cars[0]);
        }
    }

    //this only exists in this namespace. shouldnt ever be called outside.
    AssetBundle GetCarFromFile(string carName)
    {
        string filePath = Application.dataPath + "/Content/Cars/" + carName + fileExtension;
        Debug.Log("LOADED: " + filePath);
        AssetBundle corr = AssetBundle.LoadFromFile(filePath);
        Cars.Add(corr);
        return corr;
    }

    //used in car behavior on instantiate
    //used in 1000 other places
    //if you need to instantiate a car, use this. it will return the car
    //and if it was not loaded yet, will load it.
    public AssetBundle GetCar(string carName)
    {
        foreach(AssetBundle e in Cars)
        {
            if(e.name == carName)
            {
                return e;
            }
        }
        return GetCarFromFile(carName);
    }

    //use this to get a car's real name. real name is contained inside specs.json.
    //not really much i can do about this... fucking asset bundles dont support caps
    //why did i not test that beforehand, im going 9nsain
    //bool is for SAFE/UNSAFE UNLOAD.
    //"false" if you are using other parts of the car asset somewhere.
    //"true" if you want to clear it completely from memory.

    public string GetCarName(string carAlias, bool unsafeUnload)
    {
        CarData carData = new CarData();
        AssetBundle car = GetCar(carAlias);
        TextAsset carJson = car.LoadAsset("specs.json") as TextAsset;
        carData = carData.ImportData(carJson.text);

        string carname = carData.carName;
        //ok we got the car name, now just unload all the garbage i dont need
        carJson = null;
        UnloadCar(carAlias, unsafeUnload);
        return carData.carName;
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

    public void ExportHUD()
    {
        CustomHud culos = new CustomHud();
        string filePath = Application.dataPath + "/EXPORT/" + "HUD.json";
        string dataAsJson = JsonUtility.ToJson(culos, true);
        File.WriteAllText(filePath, dataAsJson);
        Debug.Log("AAAAASSSSSSSSSSSSSSSS" + filePath);
    }


}

public class Manifest
{
    public List<string> data = new List<string>();
}