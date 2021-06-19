using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataController : MonoBehaviour {
    private string gameDataFileName = "savedata.json";

    // Save data
    public GameData LoadedData;

    // Game state
    public RaceResults RaceResults;

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
    public LapTimeDictionary BestestLapTimes
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
    public DirtinessDictionary Dirtiness
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
    public bool QuickCarSelect {
        get {
            return LoadedData.QuickCarSelect;
        }
        set {
            LoadedData.QuickCarSelect = value;
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
    public bool IsBusrider {
        get {
            return LoadedData.IsBusrider;
        }
        set {
            LoadedData.IsBusrider = value;
        }
    }
    #endregion

    public float GetDirtiness() {
        float ret;
        if(!Dirtiness.TryGetValue(SelectedCar, out ret)) {
            ret = 0f;
            Dirtiness[SelectedCar] = 0f;
        }
        return ret;
    }

    public void AddDirtiness(float newDirt) {
        Dirtiness[SelectedCar] += newDirt;
    }

    public void SetDirtiness(float newDirt) {
        Dirtiness[SelectedCar] = newDirt;
    }

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

            SaveGameData();
        } else {
            Debug.LogError("Cannot load game data! Creating new gamedata file.");
            LoadedData = new GameData();
            SelectedCar = "Star GT V8";
            SelectedCourse = "Course 1";
            
            PlayerName = "Player";
            Cash = 100;
            Dirtiness = new DirtinessDictionary();
            QuickCarSelect = false;
            MenuAudio = 0.4f;
            BestestLapTimes = new LapTimeDictionary() { { "Star GT V8", TimeSpan.FromSeconds(69) }, { "Nasan GRT", TimeSpan.FromSeconds(420) } };
            IsBusrider = true;
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
    public LapTimeDictionary BestestLapTimes;

    public bool QuickCarSelect;
    public float MenuAudio;
    public bool CuntUI;
    public bool IsBusrider;
    
    public DirtinessDictionary Dirtiness;
    public int Cash;

    public bool IsMultiplayer;
    public string IP;
    public string PlayerName;
}

public class RaceResults {
    public List<Laptime> Laptimes;
}

[Serializable]
public struct Laptime : IComparable<Laptime> {
    public double ms;
    private TimeSpan _its;
    private TimeSpan its {
        get { if (_its.TotalMilliseconds == 0) {
                _its = TimeSpan.FromMilliseconds(ms);
            }
            return _its;
        }
        set { _its = its; }
    }

    public int Seconds { get { return its.Seconds; } }
    public int Minutes { get { return its.Minutes; } }
    public int Milliseconds { get { return its.Milliseconds; } }

    public int Compare(Laptime x, Laptime y) {
        return ((TimeSpan)x).CompareTo(y);
    }

    public int CompareTo(Laptime other) {
        return Compare(this, other);
    }

    public static implicit operator TimeSpan(Laptime lp) {
        return TimeSpan.FromMilliseconds(lp.ms);
    }

    public static implicit operator Laptime(TimeSpan ts) {
        return new Laptime {
            ms = ts.TotalMilliseconds,
            its = ts
        };
    }
}

[Serializable]
public class LapTimeDictionary : SerializableDictionary<string, Laptime> { };

[Serializable]
public class DirtinessDictionary : SerializableDictionary<string, float> { };

[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver {
    [SerializeField]
    private List<TKey> keys = new List<TKey>();

    [SerializeField]
    private List<TValue> values = new List<TValue>();

    // save the dictionary to lists
    public void OnBeforeSerialize() {
        keys.Clear();
        values.Clear();
        foreach (KeyValuePair<TKey, TValue> pair in this) {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }

    // load dictionary from lists
    public void OnAfterDeserialize() {
        this.Clear();

        if (keys.Count != values.Count)
            throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable.", keys.Count, values.Count));

        for (int i = 0; i < keys.Count; i++)
            this.Add(keys[i], values[i]);
    }
}