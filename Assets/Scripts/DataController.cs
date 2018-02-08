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
    #endregion


    void Start () {
        DontDestroyOnLoad(gameObject);
        LoadGameData();
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
            SelectedCar = "Carro";
            SelectedCourse = "Course 1";

            TireBias = 0;
            FinalDrive = 0;
            Aero = 1;
            SpringStiffness = 100;
            BrakeStiffness = 0;
            Gearbox = 0;

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

    public float TireBias;
    public float FinalDrive;
    public float Aero;
    public float SpringStiffness;
    public float BrakeStiffness;
    public float Gearbox;
}
