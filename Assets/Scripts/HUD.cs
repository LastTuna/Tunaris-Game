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

        RPMpointer = GetPointerRect("HUD_RPM_POINTER");
        speedPointer = GetPointerRect("HUD_SPEED_POINTER");
        turboPointer = GetPointerRect("HUD_TURBO_POINTER");

        DigitalDummies("HUD_DIGITAL_RPM_", digitalRPMdummies);
        DigitalDummies("HUD_DIGITAL_SPEED_", digitalSpeedDummies);
        DigitalDummies("HUD_DIGITAL_TURBO_", digitalTurboDummies);

        gearDisplay = GameObject.Find("HUD_GEAR").GetComponent<Text>();
        speedDisplay = GameObject.Find("HUD_SPEED").GetComponent<Text>();

        digitalSpeedPrecalc = CustomHudData.SpeedoMaxValue / digitalSpeedDummies.Count;

        //instantiate mat - TREAD WEAR READOUT
        treadDisplay = new Image[4];
        treadDisplay[0] = GameObject.Find("HUD_TREAD_FL").GetComponent<Image>();
        treadDisplay[1] = GameObject.Find("HUD_TREAD_FR").GetComponent<Image>();
        treadDisplay[2] = GameObject.Find("HUD_TREAD_RL").GetComponent<Image>();
        treadDisplay[3] = GameObject.Find("HUD_TREAD_RR").GetComponent<Image>();

        treadColors = new Material[4];
        int i = 0;
        foreach(Image m in treadDisplay)
        {
            treadColors[i] = new Material(m.material);
            treadDisplay[i].material = treadColors[i];
            i++;
        }
    }

    //need 2 check da dummies or it will refuse to execute any code underneath
    //because we are using GetComponent();
    public RectTransform GetPointerRect(string name)
    {
        if(GameObject.Find(name) != null)
        {
            return GameObject.Find(name).GetComponent<RectTransform>();
        }
        return null;
    }

    //HUD_DIGITAL_RPM_, HUD_DIGITAL_SPEED_, HUD_DIGITAL_TURBO_
    public void DigitalDummies(string type, List<GameObject> dummylist)
    {
        if (GameObject.Find(type + "0") != null)
        {
            GameObject dolor;
            for (int t = 0; true; t++)
            {
                dolor = GameObject.Find(type + t);
                if (dolor == null) break;//Break loop once run out of dummies
                dummylist.Add(GameObject.Find(type + t));
            }
        }
        else
        {
            //if nothings found add a null to first index so it can be null checked
            dummylist.Add(null);
        }
    }
    //called from CarBehavior. give the UI data json file. if not available then initiate default HUD.

        //CALL "default" TO INITIATE DEFAULT UI.
    public void GetUIinfo(string dataAsJson)
    {
        CustomHudData = new CustomHud();
        if (dataAsJson == "default")
        {
            //if u make the default UI more complex then u gotta expand on the defaults.
            //technically this is not necessary anymore but keeping it here just in case...
            CustomHudData.RPM.dialStartRot = 90;
            CustomHudData.RPM.dialEndRot = -180;
        }
        else
        {
            CustomHudData = JsonUtility.FromJson<CustomHud>(dataAsJson);
        }
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
            RPMdisplay.text = Mathf.Round(Mathf.Abs(engineRPM * CustomHudData.ReadoutRPMscale)).ToString();
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

[System.Serializable]
public class CustomHud
{
    //cant use a struct, must be class to serialize to unitys AAASSSSSSSSS JSON export...
    [System.Serializable]
    public class Dial
    {
        public float dialStartRot = 90;//start position of the dial
        public float dialEndRot = -180;//end position of the dial
    }
    public Dial RPM = new Dial();
    public Dial Speedo = new Dial();
    public Dial Turbo = new Dial();
    public float SpeedoMaxValue = 200;//reference value to the cars "top speed" on the speedo.
    public float PeakBoostValue = 1.8f;//reference value to turbo peak boost
    public float ReadoutRPMscale = 1;//RPM multiplier if you want rpm readout in digits
}
