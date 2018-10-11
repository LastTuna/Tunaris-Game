using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wipers : MonoBehaviour {
    public bool wiping = false;
    public float speed;
    public float angle;
    public bool despacito;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (wiping)
        {
            Wiper();
        }


    }
    void Wiper()
    {
        if (angle >= 90)
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
            gameObject.transform.localRotation *= Quaternion.AngleAxis(speed, new Vector3(0, 1, 0));
        }
        else
        {
            angle += -speed;
            gameObject.transform.localRotation *= Quaternion.AngleAxis(speed, new Vector3(0, -1, 0));
        }

    }
    
}
