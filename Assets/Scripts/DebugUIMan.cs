using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugUIMan : MonoBehaviour {

    public GameObject child;
    //activate/deactivate child to enable/disable debug UI
    public CanvasGroup console, general, tires, brake;

    Text engineRPM, engineOutput, boost, airSpeed;
    TireInfo FL, FR, RL, RR;
    CarBehaviour car;
    public int currentUI = 0;
    
    //0 - janny UI (extended chatwindow, quick access buttons)
    //1 - general/engine debug
    //2 - tires
    //3 - brake debug(?) - maybe for if making ABS or something else funny
    
	void Start () {
        car = FindObjectOfType<CarBehaviour>();
        FL = new TireInfo("FL");
        FR = new TireInfo("FR");
        RL = new TireInfo("RL");
        RR = new TireInfo("RR");
    }

	// Update is called once per frame
	void Update ()
    {
        //enable/disable UI.
        Switcher();

        console.alpha = 0;
        general.alpha = 0;
        tires.alpha = 0;
        brake.alpha = 0;
        
        if (child.activeSelf)
        {
            switch (currentUI)
            {
                case 0:
                    console.alpha = 1;

                    break;
                case 1:
                    general.alpha = 1;
                    GeneralUI();
                    break;
                case 2:
                    tires.alpha = 1;
                    TireUI();
                    break;
                case 3:

                    brake.alpha = 1;
                    TireUI();
                    break;
            }
        }
	}

    void Switcher()
    {
        if (Input.GetKeyDown("t"))
        {
            EnableDisable();
        }
        if (Input.GetKeyDown("y") && currentUI != 0)
        {
            currentUI--;
        }
        if (Input.GetKeyDown("u") && currentUI != 3)
        {
            currentUI++;
        }
    }


    //simple as
    public void EnableDisable()
    {
        if (child.activeSelf)
        {
            child.SetActive(false);
        }
        else
        {
            child.SetActive(true);
        }
    }

    public void GeneralUI()
    {
        //get da mf ui text n shit
        if (engineRPM == null)
        {
            engineRPM = GameObject.Find("DEBUG_engineRPM").GetComponent<Text>();
            engineOutput = GameObject.Find("DEBUG_engineOUT").GetComponent<Text>();
            boost = GameObject.Find("DEBUG_BOOST").GetComponent<Text>();
            airSpeed = GameObject.Find("DEBUG_AIRSPEED").GetComponent<Text>();
        }
        engineRPM.text = Mathf.Round(car.engineRPM).ToString();
        engineOutput.text = Mathf.Round(car.engineOUT).ToString();
        boost.text = (Mathf.Round(car.turboSpool * 10) / 10).ToString();
        airSpeed.text = (Mathf.Round(car.airSpeed * 10) / 10).ToString();
    }

    public void TireUI()
    {
        //get da wheels if u havent yet
        //gets them from car behavior.
        if(FL.tire == null)
        {
        FL.tire = car.wheelFL.gameObject.GetComponent<TireBehavior>();
        FR.tire = car.wheelFR.gameObject.GetComponent<TireBehavior>();
        RL.tire = car.wheelRL.gameObject.GetComponent<TireBehavior>();
        RR.tire = car.wheelRR.gameObject.GetComponent<TireBehavior>();

        //n also get they UI elements.
        FL.GetTexts();
        FR.GetTexts();
        RL.GetTexts();
        RR.GetTexts();
        }

        FL.RefreshData();
        FR.RefreshData();
        RL.RefreshData();
        RR.RefreshData();
    }
}

class TireInfo
{
    public TireBehavior tire;
    public string tireName;
    Text RPM;
    Text wear;
    Text fwdFric;
    Text swdFric;
    //brake debug
    Text brakeHeat;
    Text brakeTorq;
    
    //null em out just in case something duznt play nice
    public TireInfo(string name)
    {
        tire = null;
        tireName = name;
    }

    public void GetTexts()
    {
        RPM = GameObject.Find(tireName + "_DEBUG_RPM").GetComponent<Text>();
        wear = GameObject.Find(tireName + "_DEBUG_WEAR").GetComponent<Text>();
        fwdFric = GameObject.Find(tireName + "_DEBUG_fwdFRIC").GetComponent<Text>();
        swdFric = GameObject.Find(tireName + "_DEBUG_swdFRIC").GetComponent<Text>();
        brakeHeat = GameObject.Find(tireName + "_DEBUG_brakeHEAT").GetComponent<Text>();
        brakeTorq = GameObject.Find(tireName + "_DEBUG_brakeTORQ").GetComponent<Text>();
    }

    public void RefreshData()
    {
        RPM.text = Mathf.Round(tire.wheelrpm).ToString();
        wear.text = Mathf.Round(tire.TreadHealth).ToString();
        fwdFric.text = (Mathf.Round(tire.forwardSlip * 10) / 10).ToString();
        swdFric.text = (Mathf.Round(tire.sidewaysSlip * 10) / 10).ToString();
        brakeHeat.text = Mathf.Round(tire.brakeHeat).ToString();
        brakeTorq.text = Mathf.Round(tire.BrakeOutput()).ToString();
    }
}
