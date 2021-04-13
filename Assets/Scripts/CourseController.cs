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
    public List<Transform> pits;
    public List<Transform> grid;


    // AI Waypoints
    public GameObject[] AIWaypoints;

    void Start () {
        DataController dataController = FindObjectOfType<DataController>();
        if(dataController == null) {
            dataController = Instantiate(DataControllerPrefab).GetComponent<DataController>();
            dataController.GetComponent<DataController>().LoadGameData();
        }
        settings = dataController.GetComponent<DataController>().LoadedData;

        //get all pits.
        for(int i = 0; true; i++) {
            GameObject pitsearch = GameObject.Find("PIT" + i);
            if(pitsearch == null)
            {
                break;
            }
            pits.Add(pitsearch.transform);
        }
        //get all grid
        for (int i = 0; true; i++)
        {
            GameObject gridsearch = GameObject.Find("GRID" + i);
            if (gridsearch == null)
            {
                break;
            }
            grid.Add(gridsearch.transform);
        }

        ContentManager cm = FindObjectOfType<ContentManager>();
        //replace tempcar with datacontroller car name
        
        GameObject corr = Instantiate(cm.Cars[0].LoadAsset("tempcar.prefab") as GameObject, pits[0].position, pits[0].rotation);
        
        Camera.GetComponent<CarCamera>().car = corr.transform;
        corr.AddComponent<CarBehaviour>();


    }

    public void OnRaceStart() {

    }

}
