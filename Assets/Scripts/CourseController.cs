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

    public GameObject[] cars;


    // AI Waypoints
    public GameObject[] AIWaypoints;

    void Start () {
        DataController dataController = FindObjectOfType<DataController>();
        if(dataController == null) {
            dataController = Instantiate(DataControllerPrefab).GetComponent<DataController>();
            dataController.GetComponent<DataController>().LoadGameData();
        }
        settings = dataController.GetComponent<DataController>().LoadedData;

        ContentManager cm = FindObjectOfType<ContentManager>();


        Transform pitpos0 = GameObject.Find("PIT0").gameObject.transform;
        GameObject corr = Instantiate(cm.Cars[0].LoadAsset("tempcar") as GameObject, pitpos0.position, pitpos0.rotation);
        Camera.GetComponent<CarCamera>().car = corr.transform;
        corr.AddComponent<CarBehaviour>();


    }

    public void OnRaceStart() {

    }

}
