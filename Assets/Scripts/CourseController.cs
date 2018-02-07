using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourseController : MonoBehaviour {
    // Settings passed from the menus
    public GameData settings;

    // Are we loading into multiplayer
    public bool IsMultiplayer = true;
    // Server IP, null if hosting
    public string IP = null;
    // Multiplayer prefab to instantiate, should contain shit like spawn points
    public GameObject MultiplayerPrefab;

	void Start () {
        GameObject dataController = GameObject.Find("DataController");
        if (dataController != null) {
            settings = dataController.GetComponent<DataController>().LoadedData;

            // This is an example of how to access data set by the UI in the course scene
            // Sadly, it only works in C# because of a compile order snafu
            // So this controller may have to do all the heavy lifting around the unityscript car controller
            // btw, unityscript has been officially deprecated so you should really move to C#. really.
            Debug.Log(settings.SelectedCar);

            // Get multiplayer-related data
            // IsMultiplayer = settings.IsMultiplayer;
            // IP = settings.IP;
        }

        // Initialize network-related resources
        if (IsMultiplayer) {
            // Create all the spawn points
            GameObject MpPrefab = Instantiate(MultiplayerPrefab);

            // Start hosting
            MpPrefab.GetComponentInChildren<TGNetworkManager>().StartHost();
        }
    }
}
