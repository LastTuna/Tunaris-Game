using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour {
    //for if needed fixed rate rotation
    public float rotSpeed = 1.0f;
    public Vector3 rotDir = new Vector3(0, 1, 0);
    public bool fixedTime = false;//call fixed update
    public int framerate = 3;//every x update spinnering(fixed)
    int counter = 0;

    void Update () {
        if (!fixedTime)
        {
            gameObject.transform.Rotate(rotDir * rotSpeed);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (counter > framerate)
        {
            counter = 0;
            gameObject.transform.Rotate(rotDir * rotSpeed);
        }
        counter++;
    }
}
