using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//BASE CLASS FOR ALL HUDS.
//LOOK FOR A HUD DATA JSON FILE. IF ONE DUZNT EXIST THEN INITIATE DEFAULT UI.




public class HUD : MonoBehaviour {

    CustomHud CustomHudData;
    public RectTransform RPMpointer;
    public RectTransform speedPointer;
    public RectTransform turboPointer;
    public List<GameObject> digitalRPMdummies;
    float digitalSpeedPrecalc;
    public List<GameObject> digitalSpeedDummies;
    public List<GameObject> digitalTurboDummies;

    public Text gearDisplay;
    public Text speedDisplay;
    public Text RPMdisplay;
    public Image[] treadDisplay;
    private Material[] treadColors;

    void Start()
    {
        //ok so first get them dummies and slap em to whatever relevant data.
        digitalRPMdummies = new List<GameObject>();
        digitalSpeedDummies = new List<GameObject>();
        digitalTurboDummies = new List<GameObject>();

        RPMpointer = GameObject.Find("HUD_RPM_POINTER").GetComponent<RectTransform>();
        speedPointer = GameObject.Find("HUD_SPEED_POINTER").GetComponent<RectTransform>();
        turboPointer = GameObject.Find("HUD_TURBO_POINTER").GetComponent<RectTransform>();

        DigitalDummies("HUD_DIGITAL_RPM_", digitalRPMdummies);
        DigitalDummies("HUD_DIGITAL_SPEED_", digitalSpeedDummies);
        DigitalDummies("HUD_DIGITAL_TURBO_", digitalTurboDummies);

        gearDisplay = GameObject.Find("HUD_GEAR").GetComponent<Text>();
        speedDisplay = GameObject.Find("HUD_SPEED").GetComponent<Text>();

        digitalSpeedPrecalc = CustomHudData.SpeedoMaxValue / digitalSpeedDummies.Count;
        
        //instantiate mat - TREAD WEAR READOUT
        treadColors = new Material[4];
        int i = 0;
        foreach(Image m in treadDisplay)
        {
            treadColors[i] = new Material(m.material);
            treadDisplay[i].material = treadColors[i];
            i++;
        }
    }

    //HUD_DIGITAL_RPM_, HUD_DIGITAL_SPEED_, HUD_DIGITAL_TURBO_
    public void DigitalDummies(string type, List<GameObject> dummylist)
    {
        if (GameObject.Find(type + "0") == null)
        {
            GameObject dolor;
            for (int t = 0; true; t++)
            {
                dolor = GameObject.Find(type + t);
                if (dolor == null) break;//Break loop once run out of dummies
                dummylist.Add(GameObject.Find(type + t));
            }
        }
    }
    //called from CarBehavior. give the UI data json file. if not available then initiate default HUD.

    public void GetUIinfo(string dataAsJson)
    {
        CustomHudData = JsonUtility.FromJson<CustomHud>(dataAsJson);
    }

    public void UpdateHUD(float engineRPM, float engineREDLINE, float turboPressure, float currentSpeed, bool shifting, int gear, float[] treadWear) {
        float speedFactor = 0;//dial rot factor
        float rotationAngle = 0;

        //RPM DIAL
        if (engineRPM >= 0 && RPMpointer != null) {
            speedFactor = engineRPM / engineREDLINE; //RPM dial rotation
            rotationAngle = Mathf.Lerp(CustomHudData.RPM.dialStartRot, CustomHudData.RPM.dialEndRot, speedFactor);
            RPMpointer.eulerAngles = new Vector3(0, 0, rotationAngle);
        }

        //SPEED DIAL
        if (currentSpeed >= 0 && speedPointer != null)
        {
            speedFactor = currentSpeed / CustomHudData.SpeedoMaxValue;
            rotationAngle = Mathf.Lerp(CustomHudData.Speedo.dialStartRot, CustomHudData.Speedo.dialEndRot, speedFactor);
            speedPointer.eulerAngles = new Vector3(0, 0, rotationAngle);
        }

        //TURBO DIAL
        //figure out something for the interpolation value here too. (replace "speedfactor")
        if (turboPointer != null)
        {
            rotationAngle = Mathf.Lerp(CustomHudData.Turbo.dialStartRot, CustomHudData.Turbo.dialEndRot, speedFactor);
            speedPointer.eulerAngles = new Vector3(0, 0, rotationAngle);
        }

        //DIGITAL SPEED
        if(digitalSpeedDummies[0] != null)
        {
            int activeTicks = 0;
            //first turn all dummies inactive
            foreach(GameObject tick in digitalSpeedDummies)
            {
                tick.SetActive(false);
            }
            //then lit up the relevant amount of dummies
            do
            {
                digitalSpeedDummies[activeTicks].SetActive(true);

            } while (digitalSpeedPrecalc * (float)activeTicks < currentSpeed);
        }

        //DIGITAL RPM
        if (digitalRPMdummies[0] != null)
        {
            int activeTicks = 0;
            //first turn all dummies inactive
            foreach (GameObject tick in digitalRPMdummies)
            {
                tick.SetActive(false);
            }
            //then lit up the relevant amount of dummies
            do
            {
                digitalRPMdummies[activeTicks].SetActive(true);

            } while ((engineRPM / digitalRPMdummies.Count) * activeTicks <= engineRPM);
        }

        //DIGITAL TURBO
        if (digitalTurboDummies[0] != null)
        {
            int activeTicks = 0;
            //first turn all dummies inactive
            foreach (GameObject tick in digitalTurboDummies)
            {
                tick.SetActive(false);
            }
            //then lit up the relevant amount of dummies
            do
            {
                digitalTurboDummies[activeTicks].SetActive(true);

            } while ((turboPressure / digitalTurboDummies.Count) * activeTicks <= CustomHudData.PeakBoostValue);
        }

        //SPEED DISPLAY
        if (speedDisplay != null)
        {
            speedDisplay.text = Mathf.Abs(currentSpeed).ToString();
        }

        //RPM DISPLAY
        if (RPMdisplay != null)
        {
            RPMdisplay.text = Mathf.Abs(engineRPM).ToString();
        }

        //GEAR DISPLAY
        if (gearDisplay != null)
        {
            if (shifting)
            {
                gearDisplay.text = "-";
            }
            else
            {
                //gears
                if (gear == 0)
                {
                    gearDisplay.text = "R".ToString();//reverse gear
                }
                else if (gear == 1)
                {
                    gearDisplay.text = "N".ToString();//neutral
                }
                else
                {
                    gearDisplay.text = (gear - 1).ToString();//array value, minus 1
                }
            }
        }

        //TIRE READOUT
        if(treadColors[0] != null)
        {
            for (int i = 0; i < 4; i++)
            {
                //change coler
                treadColors[i].SetColor("_Color", new Color(2 * treadWear[i] / 255, treadWear[i] / 255, treadWear[i] / 255));
            }
        }
    }
}

public class CustomHud
{
    public struct Dial
    {
        public float dialStartRot;//start position of the dial
        public float dialEndRot;//end position of the dial
    }
    public Dial RPM;
    public Dial Speedo;
    public Dial Turbo;
    public float SpeedoMaxValue;//reference value to the cars "top speed" on the speedo.
    public float PeakBoostValue;//reference value to turbo peak boost
    public float ReadoutRPMscale;//RPM multiplier if you want rpm readout in digits
}
