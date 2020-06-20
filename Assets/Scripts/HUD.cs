using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour {
    public RectTransform pointer;
    public Text gearDisplay;
    public Text speedDisplay;
    public Image[] treadDisplay;
    private Material[] treadColors;
    public Text[] tirerpmText;

    void Start()
    {
        //instantiate mat
        treadColors = new Material[4];
        int i = 0;
        foreach(Image m in treadDisplay)
        {
            treadColors[i] = new Material(m.material);
            treadDisplay[i].material = treadColors[i];
            i++;
        }
    }


    public virtual void UpdateHUD(float engineRPM, float engineREDLINE, float currentSpeed, bool shifting, int gear, float[] treadWear, float[] wheelrpm) {
        float speedFactor = engineRPM / engineREDLINE; //dial rotation
        float rotationAngle = 0;
        if (engineRPM >= 0) {
            rotationAngle = Mathf.Lerp(90, -180, speedFactor);
            pointer.eulerAngles = new Vector3(0, 0, rotationAngle);
        }//end dial rot
        
        //sp33d
        speedDisplay.text = Mathf.Abs(currentSpeed).ToString();

        if (shifting) {
            gearDisplay.text = "-";
        } else {
            //gears
            if (gear == 0) {
                gearDisplay.text = "R".ToString();//reverse gear
            } else if (gear == 1) {
                gearDisplay.text = "N".ToString();//neutral
            } else {
                gearDisplay.text = (gear - 1).ToString();//array value, minus 1
            }
        }
        
        for(int i = 0; i < 4; i++)
        {
            //change coler
            treadColors[i].SetColor("_Color", new Color(2 * treadWear[i] / 255, treadWear[i] / 255, treadWear[i] / 255));
            //update wheelrpm
            tirerpmText[i].text = Mathf.Round(wheelrpm[i]).ToString();
        }




    }
}