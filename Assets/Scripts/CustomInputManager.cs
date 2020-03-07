using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//  https://answers.unity.com/questions/263881/is-there-a-way-to-manually-change-the-inputaxis-va.html
public static class CustomInput {
    static Dictionary<string, float> inputs = new Dictionary<string, float>();

    static public float GetAxis(string _axis) {
        if (!inputs.ContainsKey(_axis)) {
            inputs.Add(_axis, 0);
        }

        return inputs[_axis];
    }

    static public void SetAxis(string _axis, float _value) {
        if (!inputs.ContainsKey(_axis)) {
            inputs.Add(_axis, 0);
        }

        inputs[_axis] = _value;
    }
}