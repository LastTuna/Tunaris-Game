using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataController : MonoBehaviour {
    private string gameDataFileName = "savedata.json";
    /*  public float TireBias;
    public float FinalDrive;
    public float Aero;
    public float SpringStiffness;
    public float BrakeStiffness;
    public float Gearbox;
    */
    public GameData LoadedData;

    #region shortcuts for loaded data
    public string SelectedCar {
        get {
            return LoadedData.SelectedCar;
        }
        set {
            LoadedData.SelectedCar = value;
        }
    }
    public string SelectedCourse {
        get {
            return LoadedData.SelectedCourse;
        }
        set {
            LoadedData.SelectedCourse = value;
        }
    }
    public string[] BestestLapTimes
    {
        get
        {
            return LoadedData.BestestLapTimes;
        }
        set
        {
            LoadedData.BestestLapTimes = value;
        }
    }
    public float TireBias {
        get {
            return LoadedData.TireBias;
        }
        set {
            LoadedData.TireBias = value;
        }
    }
    public float FinalDrive {
        get {
            return LoadedData.FinalDrive;
        }
        set {
            LoadedData.FinalDrive = value;
        }
    }
    public float Aero {
        get {
            return LoadedData.Aero;
        }
        set {
            LoadedData.Aero = value;
        }
    }
    public float SpringStiffness {
        get {
            return LoadedData.SpringStiffness;
        }
        set {
            LoadedData.SpringStiffness = value;
        }
    }
    public float BrakeStiffness {
        get {
            return LoadedData.BrakeStiffness;
        }
        set {
            LoadedData.BrakeStiffness = value;
        }
    }
    public float Gearbox {
        get {
            return LoadedData.Gearbox;
        }
        set {
            LoadedData.Gearbox = value;
        }
    }
    public string PlayerName {
        get {
            return LoadedData.PlayerName;
        }
        set {
            LoadedData.PlayerName = value;
        }
    }
    //online;
    public string IP
    {
        get
        {
            return LoadedData.IP;
        }
        set
        {
            LoadedData.IP = value;
        }
    }
    public float[] Dirtiness
    {
        get
        {
            return LoadedData.Dirtiness;
        }
        set
        {
            LoadedData.Dirtiness = value;
        }
    }
    public int Cash
    {
        get
        {
            return LoadedData.Cash;
        }
        set
        {
            LoadedData.Cash = value;
        }
    }
    public bool Garage3D {
        get {
            return LoadedData.Garage3D;
        }
        set {
            LoadedData.Garage3D = value;
        }
    }
    public float MenuAudio {
        get {
            return LoadedData.MenuAudio;
        }
        set {
            LoadedData.MenuAudio = value;
        }
    }
    public bool CuntUI {
        get {
            return LoadedData.CuntUI;
        }
        set {
            LoadedData.CuntUI = value;
        }
    }
    #endregion


    void Start () {
        // Check existence of a previous DataController
        if (FindObjectsOfType<DataController>().Length > 1) {
            // Return to menu from game scene
            // Put player back in city area and kill ourselves
            FindObjectOfType<Controller>().GoRace();

            Destroy(this);
        } else {
            // First game load, initialize
            DontDestroyOnLoad(gameObject);
            LoadGameData();
        }
    }

    public void LoadGameData() {
        string filePath = Path.Combine(Application.dataPath, gameDataFileName);

        if (File.Exists(filePath)) {
            string dataAsJson = File.ReadAllText(filePath);
            LoadedData = JsonUtility.FromJson<GameData>(dataAsJson);

            SelectedCar = LoadedData.SelectedCar;
            SelectedCourse = LoadedData.SelectedCourse;
        } else {
            Debug.LogError("Cannot load game data! Creating new gamedata file.");
            LoadedData = new GameData();
            SelectedCar = "Star GT V8";
            SelectedCourse = "Course 1";

            TireBias = 1;
            FinalDrive = 4.3f;
            Aero = 15;
            SpringStiffness = 100;
            BrakeStiffness = 200;
            Gearbox = 0;
            PlayerName = "Player";
            Cash = 100;
            Dirtiness = new float[]{0,0,0,0,0,0,0};//change this accordingly to amount of cars ingame
            Garage3D = false;
            MenuAudio = 0.4f;
            BestestLapTimes = new string[]{"01:25:700","01:27.100"};
            SaveGameData();
        }
    }

    public void SaveGameData() {
        string dataAsJson = JsonUtility.ToJson(LoadedData);

        string filePath = Path.Combine(Application.dataPath, gameDataFileName);
        File.WriteAllText(filePath, dataAsJson);
        Debug.Log("Saved");
    }
}

[Serializable]
public class GameData {
    public string SelectedCar;
    public string SelectedCourse;
    public string[] BestestLapTimes;

    public bool Garage3D;
    public float MenuAudio;
    public bool CuntUI;

    public float TireBias;
    public float FinalDrive;
    public float Aero;
    public float SpringStiffness;
    public float BrakeStiffness;
    public float Gearbox;
    public float[] Dirtiness;
    public int Cash;

    public bool IsMultiplayer;
    public string IP;
    public string PlayerName;
}
