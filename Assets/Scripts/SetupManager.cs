using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;

//this script is linked with eventSystem Controller script.

public class SetupManager : MonoBehaviour {

    public SetupUI[] CarUI_List;
    private Canvas loadedCanvas;//loaded canvas goes here so it can be destroyed
                                //just move the ui list values to this dict ig...

    public SetupData currentSetup;
    public SetupRestrictions Restrictions;//load the car specific regs/defaults

    //this is called when you open setup menu
    //load cars relevant UI
    public void LoadSetupUI(string carName)
    {
        int i = 0;
        //just sift through all the options till you find the relevant option
        //cannot use dict because would need to serialize it or sth
        while(i < CarUI_List.Length)
        {
            if (carName == CarUI_List[i].CarName) break;
            i++;
        }
        loadedCanvas = Instantiate(CarUI_List[i].CarCanvas);
        //car setup restrictions are included in the game object connected to the canvas
        Restrictions = FindObjectOfType<SetupRestrictions>();
        //if car was changed then load the default setup for that car
        if (currentSetup.CarName != CarUI_List[i].CarName)
        {
            LoadSetup("default");
        }
        else
        {
            LoadSetup("currentSetup");
        }

    }

    public void LoadDefaultSetup()
    {
        //get default setup for this car from carSetupRestrictions
        //bruh moment!!!!!!!
        currentSetup.CarName = Restrictions.CarName;
        currentSetup.Aero = Restrictions.GetDefaultValue("Aero");
        currentSetup.LSD = Restrictions.GetDefaultValue("LSD");
        currentSetup.TorqueSplit = Restrictions.GetDefaultValue("TorqueSplit");
        currentSetup.SteerLock = Restrictions.GetDefaultValue("SteerLock");
        currentSetup.FrontSpringHeight = Restrictions.GetDefaultValue("FrontSpringHeight");
        currentSetup.FrontSpringStiffness = Restrictions.GetDefaultValue("FrontSpringStiffness");
        currentSetup.FrontDamperStiffness = Restrictions.GetDefaultValue("FrontDamperStiffness");
        currentSetup.RearSpringHeight = Restrictions.GetDefaultValue("RearSpringHeight");
        currentSetup.RearSpringStiffness = Restrictions.GetDefaultValue("RearSpringStiffness");
        currentSetup.RearDamperStiffness = Restrictions.GetDefaultValue("RearDamperStiffness");
        currentSetup.BrakeStrength = Restrictions.GetDefaultValue("BrakeStrength");
        currentSetup.BrakeBalance = Restrictions.GetDefaultValue("BrakeBalance");
        currentSetup.GearRatios = Restrictions.gearbox.defaultRatios;
        currentSetup.FinalDrive = Restrictions.gearbox.defaultFinalDrive;
    }

    public void UnLoadSetupUI()
    {
        GetSliderValues();
        Destroy(loadedCanvas.gameObject);
    }

    //load the data related to all the setups in the folder.
    public void LoadSetupsList()
    {
        string filePath = Application.dataPath + "/setups/" + currentSetup.CarName + "/";
        Debug.Log(filePath);

        //get all the files in the folder
        string[] penus = Directory.GetFiles(filePath);
        foreach(string dolor in penus)
        {
            //chcek that its a json from filename then it should be displayed via PrintSetups()
            if (dolor.EndsWith(".json"))
            {
                string beer = dolor.Substring(dolor.LastIndexOf("/") + 1, dolor.Length - dolor.LastIndexOf("/") - 6);
                Debug.Log(beer);
                PrintSetups(beer);

            }
            
            //Debug.Log(dolor);
        }

        //load data to list on the already saved setups ui
    }

    //the coroutine to print all the data, called from LoadSetupsList()
    public void PrintSetups(string setupName)
    {
        //prob need ot make an instance of some UI element. have the UI element have a button with the button having a param for the setup name
        //get setup description on clicking on the element. and display it in a window somewhere on the screen.
        
        //this is called for every setup individually ig
    }

    //use this to load a picked setup
    public void LoadSetup(string setupName)
    {
        //string filePath = Application.dataPath + "/setups/" + currentSetup.CarName + "/" + setupName + ".json";

        //default handler here
        if (setupName == "default")
        {
            LoadDefaultSetup();
            SetSliderValues(currentSetup);
        }
        else if(setupName == "currentSetup")
        {
            SetSliderValues(currentSetup);
        }
        else
        {
            //loadeing from json
            currentSetup = JsonUtility.FromJson<SetupData>(setupName);
            SetSliderValues(currentSetup);
        }
        
    }

    public void SetSliderValues(SetupData newSetup)
    {
        //just take all available sliders from restrictions and slap the relevant value to them
        Restrictions.SetSlider("Aero", newSetup.Aero);
        Restrictions.SetSlider("LSD", newSetup.LSD);
        Restrictions.SetSlider("TorqueSplit", newSetup.TorqueSplit);
        Restrictions.SetSlider("SteerLock", newSetup.SteerLock);
        Restrictions.SetSlider("FrontSpringHeight", newSetup.FrontSpringHeight);
        Restrictions.SetSlider("FrontSpringStiffness", newSetup.FrontSpringStiffness);
        Restrictions.SetSlider("FrontDamperStiffness", newSetup.FrontDamperStiffness);
        Restrictions.SetSlider("RearSpringHeight", newSetup.RearSpringHeight);
        Restrictions.SetSlider("RearSpringStiffness", newSetup.RearSpringStiffness);
        Restrictions.SetSlider("RearDamperStiffness", newSetup.RearDamperStiffness);
        Restrictions.SetSlider("BrakeStrength", newSetup.BrakeStrength);
        Restrictions.SetSlider("BrakeBalance", newSetup.BrakeBalance);
        Restrictions.SetSlider("FinalDrive", newSetup.FinalDrive);
        Restrictions.SetGearboxSliders(newSetup.GearRatios);
    }

    public void GetSliderValues()
    {
        //just get sliders and their values
        currentSetup.Aero = Restrictions.GetSlider("Aero");
        currentSetup.LSD = Restrictions.GetSlider("LSD");
        currentSetup.TorqueSplit = Restrictions.GetSlider("TorqueSplit");
        currentSetup.SteerLock = Restrictions.GetSlider("SteerLock");
        currentSetup.FrontSpringHeight = Restrictions.GetSlider("FrontSpringHeight");
        currentSetup.FrontSpringStiffness = Restrictions.GetSlider("FrontSpringStiffness");
        currentSetup.FrontDamperStiffness = Restrictions.GetSlider("FrontDamperStiffness");
        currentSetup.RearSpringHeight = Restrictions.GetSlider("RearSpringHeight");
        currentSetup.RearSpringStiffness = Restrictions.GetSlider("RearSpringStiffness");
        currentSetup.RearDamperStiffness = Restrictions.GetSlider("RearDamperStiffness");
        currentSetup.BrakeStrength = Restrictions.GetSlider("BrakeStrength");
        currentSetup.BrakeBalance = Restrictions.GetSlider("BrakeBalance");
        currentSetup.FinalDrive = Restrictions.GetSlider("FinalDrive");
        
        currentSetup.GearRatios = Restrictions.GetGearboxSliders(currentSetup.GearRatios);
    }

    public void SaveSetup(string nameValue, string descValue)
    {
        //collect relevant data
        SetupData SetupToSave = currentSetup;

        SetupToSave.SetupName = nameValue;
        SetupToSave.SetupDescription = descValue;

        //parse json

        string filePath = Application.dataPath + "/setups/" + currentSetup.CarName + "/" + nameValue + ".json";
        string dataAsJson = JsonUtility.ToJson(SetupToSave);
        File.WriteAllText(filePath, dataAsJson);
        Debug.Log("Saved setup " + nameValue + ".json");
    }
    

    //returns true if a file with that name exists
    public bool SetupDuplicateChecker(string search)
    {
        string filePath = Application.dataPath + "/setups/" + currentSetup.CarName + "/";
        DirectoryInfo carSetupDir = new DirectoryInfo(filePath);
        FileInfo[] fileInfo = carSetupDir.GetFiles();
        foreach(FileInfo culo in fileInfo)
        {
            if (culo.Name == search + ".json") return true;
        }
        return false;
    }


}


[Serializable]
public class SetupData
{
    public string CarName;
    public string SetupName;
    public string SetupDescription;

    public float Aero;
    public float LSD;
    public float TorqueSplit;
    public float SteerLock;
    public float FrontSpringHeight;
    public float RearSpringHeight;
    public float FrontSpringStiffness;
    public float RearSpringStiffness;
    public float FrontDamperStiffness;
    public float RearDamperStiffness;
    public float BrakeStrength;
    public float BrakeBalance;
    public float[] GearRatios;
    public float FinalDrive;

    public int Gearbox;
}

[Serializable]
public class SetupUI
{
    public string CarName;
    public Canvas CarCanvas;
}