using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourseController : MonoBehaviour {
    public GameData settings;

	void Start () {
        settings = GameObject.Find("DataController").GetComponent<DataController>().LoadedData;

        Debug.Log(settings.SelectedCar);
        Debug.Log(settings.SelectedCourse);
    }
}
