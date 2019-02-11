using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutismAnalyzer : MonoBehaviour {

    public AnimationCurve speedCurve = new AnimationCurve();//x wheel speed , y air speed
    public CarBehaviour carro;
    public float carroSpeed;
    public float airoSpeed;


    // Use this for initialization
    void Start () {
        carro = FindObjectOfType<CarBehaviour>();
	}
	
	// Update is called once per frame
	void Update () {
        carroSpeed = Mathf.RoundToInt(carro.currentSpeed);
        airoSpeed = Mathf.RoundToInt(carro.airSpeed);

        if(carroSpeed % 10 == 0)
        {
            speedCurve.AddKey(carroSpeed, airoSpeed);
        }
		
	}
}
