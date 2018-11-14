using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wipers : MonoBehaviour {
    public bool wiping = false;
    public float speed = 3;
    public float angle;
    public float maxAngle = 90;//Default 90
    public bool despacito;
    public float e;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (wiping || angle > 0)
        {
            Wiper();
        }
        if (Input.GetAxis("Wipers") > 0 && !wiping)
        {
            wiping = true;
        }
        else if (Input.GetAxis("Wipers") > 0 && wiping)
        {
            wiping = false;
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
            gameObject.transform.localRotation *= Quaternion.AngleAxis(speed * (maxAngle / 90), new Vector3(0, 0, 1));
        }
        else
        {
            angle += -speed;
            gameObject.transform.localRotation *= Quaternion.AngleAxis(speed * (maxAngle / 90), new Vector3(0, 0, -1));
        }
        
        




 }
}
