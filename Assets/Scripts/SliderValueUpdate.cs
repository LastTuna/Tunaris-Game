using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderValueUpdate : MonoBehaviour {
    public Slider slider;
    public Text value;

    public void ValueUpdated() {
        value.text = slider.value.ToString();
    }
}
