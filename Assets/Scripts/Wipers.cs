using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wipers : MonoBehaviour {
    public bool wiping = false;
    public float speed = 180;
    public float angle;
    public float maxAngle = 90;//Default 90
    public bool despacito;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (wiping || angle > 0)
        {
            Wiper();
        }
        if (Input.GetKeyDown("h"))
        {
            wiping = true;
        }

    }
    void Wiper()
    {
        if (angle >= maxAngle)
        {
            despacito = false;
        }
        if (angle <= 0)
        {
            despacito = true;
        }
        ////
        if (despacito)
        {
            angle += speed;
            gameObject.transform.localRotation *= Quaternion.AngleAxis(speed, new Vector3(0, 0, 1));
        }
        else
        {
            angle += -speed;
            gameObject.transform.localRotation *= Quaternion.AngleAxis(speed, new Vector3(0, 0, -1));
        }

    }
    
}
