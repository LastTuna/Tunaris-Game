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
        //content manager by default exists in main menu scene
        //so making sure when returning to main menu there wont
        //be a second instance.
        if (GameObject.FindObjectOfType<ContentManager>())
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
        //id think refreshing the manifest on startup would be a pretty smart idea.
        //so add generatemanifest() here.



        //im just doing some hardcoded thing to make basic functionality again.
        //this should be looped with the data from the manifest to get
        //all the cars into the list. essentially just replace "tempcar"
        string filePath = Application.dataPath + "/Content/Cars/tempcar";
        Debug.Log(filePath);
        AssetBundle corr = AssetBundle.LoadFromFile(filePath);
        //not sure if i need to unload corr inbetween. leaving as cliffnote
        Cars.Add(corr);
    }

    //this writes the manifest
    public void GenerateManifest()
    {
        string filePath = Application.dataPath + "/content/cars";
        Debug.Log(filePath);

        //get all the files in the folder
        string[] penus = Directory.GetFiles(filePath);

        string dataAsJson = JsonUtility.ToJson(penus);
        File.WriteAllText(filePath + "/manifest.json", dataAsJson);
        Debug.Log("manifest.json has been refreshed.");
    }


    //first make a method that lists all the cars from content
    //compile to json to make a manifest.



    //just a debug func
    public void ExportData()
    {
        //export a json to EXPORT folder. has default values.
        //trigger this by whatever you want really. not used ingame.
        CarData penis = new CarData();

        string filePath = Application.dataPath + "/EXPORT/" + "penis.json";
        string dataAsJson = JsonUtility.ToJson(penis);
        File.WriteAllText(filePath, dataAsJson);
        Debug.Log("PEN IS MUSIC" + filePath);
    }



}