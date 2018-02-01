using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourseController : MonoBehaviour {
    public GameData settings;

	void Start () {
        settings = GameObject.Find("DataController").GetComponent<DataController>().LoadedData;

        // This is an example of how to access data set by the UI in the course scene
        // Sadly, it only works in C# because of a compile order snafu
        // So this controller may have to do all the heavy lifting around the unityscript car controller
        // btw, unityscript has been officially deprecated so you should really move to C#. really.
        Debug.Log(settings.SelectedCar);
    }
}
