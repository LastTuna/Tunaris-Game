using UnityEngine;
using UnityEngine.UI;

public class CarWash : MonoBehaviour {

    public DataController dataController;
    public Controller controller;
    public int carIndex = 0;
    public float dirtiness;
    public Material dirt;
    public Material[] wheelDirt = new Material[4];
    public Button washerButton;

	void Start () {
        dataController = FindObjectOfType<DataController>();
        controller = FindObjectOfType<Controller>();
        washerButton = GameObject.Find("WasherButton").GetComponent<Button>();

        dirt = GameObject.Find("DIRT").GetComponent<Renderer>().material;
        wheelDirt[0] = GameObject.Find("FLdirt").GetComponent<Renderer>().material;
        wheelDirt[1] = GameObject.Find("FRdirt").GetComponent<Renderer>().material;
        wheelDirt[2] = GameObject.Find("RLdirt").GetComponent<Renderer>().material;
        wheelDirt[3] = GameObject.Find("RRdirt").GetComponent<Renderer>().material;

        foreach (GameObject e in controller.carsPrefabs)
        {//get cars index
            if (dataController.SelectedCar.Equals(e.name))
            {
                break;
            }
            carIndex++;
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        dirtiness = dataController.Dirtiness[carIndex];
        dirt.color = new Color(1, 1, 1, dirtiness);
        foreach(Material e in wheelDirt)
        {
            e.color = new Color(1, 1, 1, dirtiness);
        }
    }

}
