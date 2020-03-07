using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CourseController : MonoBehaviour {
    public GameObject DataControllerPrefab;
    // Settings passed from the menus
    public GameData settings;

    // Are we loading into multiplayer
    public bool IsMultiplayer = false;
    // Server IP, null if hosting
    public string IP = null;
    // Per-course multiplayer prefab to instantiate, should contain shit like spawn points
    public GameObject MultiplayerPrefab;
    // Global Host UI prefab to instantiate
    public GameObject HostUI;
    public GameObject HostUIInstance;

    // Default HUD
    public GameObject DefaultHUD;
    // Busrider mode HUD
    public GameObject BusriderHUD;
    // Canvas to instantiate HUD into
    public GameObject HUDCanvas;

    // Reference to the scene's AudioSource
    public AudioSource AudioSource;
    // Reference to the scene's Camera
    public GameObject Camera;

    // NetworkManager
    public TGNetworkManager manager;

    // Instantiated multiplayer object
    private GameObject MpPrefab;

    // AI Waypoints
    public GameObject[] AIWaypoints;

    void Start () {
        DataController dataController = FindObjectOfType<DataController>();
        if(dataController == null) {
            dataController = Instantiate(DataControllerPrefab).GetComponent<DataController>();
            dataController.GetComponent<DataController>().LoadGameData();
        }
        settings = dataController.GetComponent<DataController>().LoadedData;

        // Get multiplayer-related data
        IsMultiplayer = settings.IsMultiplayer;
        IP = settings.IP;

        // Initialize network-related resources
        if (IsMultiplayer) {
            // Create all the spawn points
            MpPrefab = Instantiate(MultiplayerPrefab);
            manager = MpPrefab.GetComponentInChildren<TGNetworkManager>();
            manager.UserSettings = settings;
            manager.RaceStart = GetComponent<RaceStart>();

            if (IP == null || IP == string.Empty) {
                // Start hosting
                manager.StartHost();

                // Create host UI
                HostUIInstance = Instantiate(HostUI);
                HostUIInstance.name = "HostUI";
                HostUIInstance.GetComponentInChildren<Button>().onClick.AddListener(StartRaceProcess);
            } else {
                // Connect
                manager.networkAddress = IP;
                manager.StartClient();
            }
        }
    }

    public void OnRaceStart() {
        Destroy(HostUIInstance);
    }

    public void Cleanup() {
        // Kill the network manager when leaving the scene
        if (IP == null || IP == string.Empty) {
            manager.StopHost();
        } else {
            manager.StopClient();
        }
        Destroy(manager);
    }

    public void StartRaceProcess() {
        manager.StartRaceProcess();
    }
}
