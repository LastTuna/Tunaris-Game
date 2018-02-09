using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CourseController : MonoBehaviour {
    public GameObject DataControllerPrefab;
    // Settings passed from the menus
    public GameData settings;

    // Are we loading into multiplayer
    public bool IsMultiplayer = true;
    // Server IP, null if hosting
    public string IP = null;
    // Per-course multiplayer prefab to instantiate, should contain shit like spawn points
    public GameObject MultiplayerPrefab;
    // Global Host UI prefab to instantiate
    public GameObject HostUI;

    // References to this course's HUD
    public Text SpeedDisplayHUD;
    public Text GearDisplayHUD;
    public RectTransform PointerHUD;
    // Reference to the scene's AudioSource
    public AudioSource AudioSource;

    void Start () {
        GameObject dataController = GameObject.Find("DataController");
        if(dataController == null) {
            dataController = Instantiate(DataControllerPrefab);
            dataController.GetComponent<DataController>().LoadGameData();
        }
        settings = dataController.GetComponent<DataController>().LoadedData;

        // This is an example of how to access data set by the UI in the course scene
        // Sadly, it only works in C# because of a compile order snafu
        // So this controller may have to do all the heavy lifting around the unityscript car controller
        // btw, unityscript has been officially deprecated so you should really move to C#. really.
        Debug.Log(settings.SelectedCar);

        // Get multiplayer-related data
        // IsMultiplayer = settings.IsMultiplayer;
        // IP = settings.IP;

        // Initialize network-related resources
        if (IsMultiplayer) {
            // Create all the spawn points
            GameObject MpPrefab = Instantiate(MultiplayerPrefab);
            TGNetworkManager manager = MpPrefab.GetComponentInChildren<TGNetworkManager>();

            if (IP == null || IP == string.Empty) {
                // Start hosting
                manager.StartHost();

                // Create host UI
                GameObject HostUIInstance = Instantiate(HostUI);
                HostUIInstance.name = "HostUI";
                HostUIInstance.GetComponentInChildren<Button>().onClick.AddListener(StartRaceProcess);
            } else {
                // Connect
                manager.networkAddress = IP;
                manager.StartClient();
            }
        }
    }

    public void StartRaceProcess() {
        RaceStart rcComponent = GetComponent<RaceStart>();
        StartCoroutine(rcComponent.CountDown());
    }
}
