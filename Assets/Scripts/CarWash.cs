using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarWash : MonoBehaviour {

    public DataController dataController;
    public Controller controller;
    public int carIndex = 0;
    public float dirtiness;
    public Material dirt;
    public Button washerButton;

	void Start () {
        dataController = FindObjectOfType<DataController>();
        controller = FindObjectOfType<Controller>();
        washerButton = GameObject.Find("WasherButton").GetComponent<Button>();
        foreach (GameObject e in controller.carsPrefabs)
        {//get cars index
            if (dataController.SelectedCar.Equals(e.name))
            {
                break;
            }
            carIndex++;
        }
        dirt = GameObject.Find("dirt").GetComponent<Renderer>().material;
    }
	
	// Update is called once per frame
	void Update ()
    {
        dirtiness = dataController.Dirtiness[carIndex];
        dirt.color = new Color(200, 200, 200, dirtiness);

    }
    public void WashMe ()
    {
        dataController.Cash += -5;
        while (dataController.Dirtiness[carIndex] > 0)
        {
            dataController.Dirtiness[carIndex] += -0.001f;
        }
        if(dataController.Dirtiness[carIndex] < 0)
        {
            dataController.Dirtiness[carIndex] = 0;
        }
    }

}
