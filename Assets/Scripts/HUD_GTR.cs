using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD_GTR : HUD {
    public override void UpdateHUD(float engineRPM, float engineREDLINE, float currentSpeed, bool shifting, int gear, float[] treadWear, float[] wheelrpm) {
        float speedFactor = engineRPM / engineREDLINE; //dial rotation
        float rotationAngle = 0;
        if (engineRPM >= 0) {
            rotationAngle = Mathf.Lerp(90, -180, speedFactor);
            pointer.eulerAngles = new Vector3(0, 0, rotationAngle);
        }//end dial rot

        if (currentSpeed < 0)//cancelling negative integers, speed
        {
            speedDisplay.text = (currentSpeed * -1).ToString();
        } else {
            speedDisplay.text = currentSpeed.ToString();
        }
        if (shifting) {
            gearDisplay.text = "-";
        } else {
            //gears
            if (gear == 0) {
                gearDisplay.text = "Gang R".ToString();//reverse gear
            } else if (gear == 1) {
                gearDisplay.text = "Gang N".ToString();//neutral
            } else {
                gearDisplay.text = "Gang" + (gear - 1).ToString();//array value, minus 1
            }
        }
    }
}