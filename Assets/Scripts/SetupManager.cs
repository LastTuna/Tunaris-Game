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

    public GameObject sliderBox;//this is the gameobject of the slider box,which controls
    //the size of the setups scroll box
    GameObject sliderChild;
    //instantiate a child that will act as a parent to all the instantiated buttons.
    public GameObject loadButtonPrefab;
    //name this something intelligent when i figure out the prefab
    //will be instantiated to the slider child
    Button[] loadButtons;
    //container for all loaded buttons.
    public string selectedSetup = "default";
    //container for the selected setup in load setup screen. when buttons pressed the setup name gets assigned here
    public Text descriptionBox;
    //this is the description box in load setup screen.
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

    public void UnloadButtons()
    {
        Destroy(sliderChild);
    }

    //unload save setup ui
    public void UnLoadSetupUI()
    {
        GetSliderValues();
        Destroy(loadedCanvas.gameObject);
    }

    //load the data related to all the setups in the folder.
    public void LoadSetupsList()
    {
        sliderChild = Instantiate(new GameObject("ButtonParent"), sliderBox.transform);
        //instantiate a slider child for easy button destroying
        loadButtons = new Button[100];
        //also instantiate an array for buttons. i just set max size to like 100 because realistically
        //none will have fuckin 100 setups...right?
        string filePath = Application.dataPath + "/setups/" + currentSetup.CarName + "/";
        Debug.Log(filePath);

        //get all the files in the folder
        string[] penus = Directory.GetFiles(filePath);
        int jsonsInFolder = 0;
        for(int i = 0; i < penus.Length;i++)
        {
            //check that its a json from filename then it should be displayed via PrintSetups()
            if (penus[i].EndsWith(".json"))
            {
                string beer = penus[i].Substring(penus[i].LastIndexOf("/") + 1, penus[i].Length - penus[i].LastIndexOf("/") - 6);
                StartCoroutine(PrintSetups(beer, jsonsInFolder));
                jsonsInFolder++;
            }
            
            //Debug.Log(dolor);
        }

        //load data to list on the already saved setups ui
    }

    //the coroutine to print all the data, called from LoadSetupsList()
    public IEnumerator PrintSetups(string setupName, int i)
    {
        yield return new WaitForEndOfFrame();
        GameObject newButton = Instantiate(loadButtonPrefab, sliderChild.transform);
        newButton.transform.position += new Vector3(0, i * -20, 0);
        loadButtons[i] = newButton.GetComponent<Button>();
        loadButtons[i].GetComponentInChildren<Text>().text = setupName;
        loadButtons[i].onClick.AddListener( () => ButtonOnClick(setupName));
        yield return 0;
        //create buton. add listener to buton. chan buton text to setup name
        //get setup description on clicking on the element. and display it in a window somewhere on the screen.
    }

    public void ButtonOnClick(string choice)
    {
        //called when setup buttons selected. print the description to the funny box
        //and assign the setup name to container
        selectedSetup = choice;
        descriptionBox.text = LoadDescription(choice);

    }

    //insert the text from selected setup to the desc box.
    public string LoadDescription(string choice)
    {
        string filePath = Application.dataPath + "/setups/" + currentSetup.CarName + "/" + choice + ".json";

        string dataAsJson = File.ReadAllText(filePath);
        SetupData descData = JsonUtility.FromJson<SetupData>(dataAsJson);
        return descData.SetupDescription;
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

        //check existence of final drive slider & apply value if exist
        //this was called separately because final drive ratios are contained in the gearbox object
        if (Restrictions.gearbox.finalDriveSlider != null)
        {
            Restrictions.gearbox.finalDriveSlider.value = newSetup.FinalDrive;
        }

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

        //check existence of final drive slider & apply value if exist
        //this was called separately because final drive ratios are contained in the gearbox object
        if (Restrictions.gearbox.finalDriveSlider != null)
        {
            currentSetup.FinalDrive = Restrictions.gearbox.finalDriveSlider.value;
        }

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