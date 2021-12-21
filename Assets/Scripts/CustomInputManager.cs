using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//  https://answers.unity.com/questions/263881/is-there-a-way-to-manually-change-the-inputaxis-va.html
//this class can be used to override unitys input system
//useful for AI control + mobile touch screen controls

public static class CustomInput {

    static Dictionary<string, float> inputs = new Dictionary<string, float>();
    public static bool IsAI = false;

    static public float GetAxis(string _axis) {
        if (!inputs.ContainsKey(_axis)) {
            inputs.Add(_axis, 0);
        }
        if (!IsAI) {
            return Input.GetAxis(_axis);
        } else {
            return inputs[_axis];
        }
    }

    static public void SetAxis(string _axis, float _value) {
        if (!inputs.ContainsKey(_axis)) {
            inputs.Add(_axis, 0);
        }

        inputs[_axis] = _value;
    }
}