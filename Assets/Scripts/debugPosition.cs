using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class debugPosition : MonoBehaviour {

    public Text onscreen;



	// Use this for initialization
	void Start () {

        onscreen = GameObject.Find("X").GetComponent<Text>();

    }
	
	// Update is called once per frame
	void Update ()
    {
        onscreen.text = gameObject.transform.position.ToString();


    }
}
