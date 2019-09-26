using UnityEngine;
using UnityEngine.UI;

public class CarWash : MonoBehaviour {

    public DataController dataController;
    public Controller controller;
    public float dirtiness;
    public Material dirt;
    public Material mainbody;
    public Material[] wheelDirt = new Material[4];
    public Button washerButton;

	void Start () {
        dataController = FindObjectOfType<DataController>();
        controller = FindObjectOfType<Controller>();
        washerButton = GameObject.Find("WasherButton").GetComponent<Button>();
        //issue here
        mainbody = GameObject.Find("mainbody").GetComponent<Renderer>().material;
        mainbody.DisableKeyword("_GLOSSYREFLECTIONS_OFF");
        mainbody.SetInt("_GlossyReflections", 1);
        dirt = GameObject.Find("DIRT").GetComponent<Renderer>().material;
        wheelDirt[0] = GameObject.Find("FLdirt").GetComponent<Renderer>().material;
        wheelDirt[1] = GameObject.Find("FRdirt").GetComponent<Renderer>().material;
        wheelDirt[2] = GameObject.Find("RLdirt").GetComponent<Renderer>().material;
        wheelDirt[3] = GameObject.Find("RRdirt").GetComponent<Renderer>().material;
    }
	
	// Update is called once per frame
	void Update ()
    {

        mainbody.SetFloat("_Glossiness", 1f);//placeholder
        dirtiness = dataController.GetDirtiness();
        dirt.color = new Color(1, 1, 1, dirtiness);
        foreach(Material e in wheelDirt)
        {
            e.color = new Color(1, 1, 1, dirtiness);
        }
    }

}
