using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour {
    public float TimeScale = 60f;
    public int Hours = 12;
    public int Minutes = 00;

    private readonly float DegreesPerMinute = 0.25f;

    public SunSettings[] SunSettings;

    public GameObject SunRoot;
    public GameObject NightLightRoot;

    void Start() {
        StartCoroutine(DayNightCycleCoroutine());
    }

    IEnumerator DayNightCycleCoroutine() {
        while (true) {
            // Get settings
            SunSettings sunPosition = SunSettings[0];
            foreach (SunSettings w in SunSettings) {
                if (Hours >= w.StartHour && Hours < w.EndHour) {
                    sunPosition = w;
                    break;
                }
            }

            // Process settings
            NightLightRoot.SetActive(sunPosition.NightLightsOn);
            SunRoot.GetComponentInChildren<Light>().intensity = sunPosition.SunBrightness;
            SunRoot.transform.localEulerAngles = new Vector3(DegreesPerMinute * ((Hours * 60) + Minutes), 0, 0);
            RenderSettings.skybox = sunPosition.Skybox;
            //DynamicGI.UpdateEnvironment();

            // Increment time
            Minutes++;
            if (Minutes == 60) {
                Minutes = 0;
                Hours++;
            }
            if (Hours == 24) {
                Hours = 0;
            }

            yield return new WaitForSeconds(60 / TimeScale);
        }
    }
}

[Serializable]
public class SunSettings {
    public int StartHour;
    public int EndHour;
    public int SunBrightness;
    public bool NightLightsOn;
    public Color SunColor;
    public Material Skybox;
}
