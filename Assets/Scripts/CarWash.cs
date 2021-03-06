﻿using UnityEngine;
using UnityEngine.UI;

public class CarWash : MonoBehaviour {

    public DataController dataController;
    public Controller controller;
    public float dirtiness;
    public Material mainbody;
    public Material[] wheelDirt = new Material[4];
    public Button washerButton;

    void Start() {
        dataController = FindObjectOfType<DataController>();
        controller = FindObjectOfType<Controller>();
        washerButton = GameObject.Find("WasherButton").GetComponent<Button>();
        mainbody = GameObject.Find("mainbody").GetComponent<Renderer>().material;
        mainbody.DisableKeyword("_GLOSSYREFLECTIONS_OFF");
        mainbody.SetInt("_GlossyReflections", 1);
        wheelDirt[0] = GameObject.Find("FLwheel").GetComponent<Renderer>().material;
        wheelDirt[1] = GameObject.Find("FRwheel").GetComponent<Renderer>().material;
        wheelDirt[2] = GameObject.Find("RLwheel").GetComponent<Renderer>().material;
        wheelDirt[3] = GameObject.Find("RRwheel").GetComponent<Renderer>().material;
    }
	
	// Update is called once per frame
	void Update ()
    {
        dirtiness = dataController.GetDirtiness();
        mainbody.SetFloat("_Glossiness", Mathf.Clamp01(1 - (dirtiness * 2)));
        mainbody.SetFloat("_FortniteRange", dirtiness);
        foreach (Material e in wheelDirt)
        {
            e.SetFloat("_FortniteRange", dirtiness);
        }
    }

}
