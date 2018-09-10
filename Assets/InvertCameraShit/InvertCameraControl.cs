using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvertCameraControl : MonoBehaviour {
    public GameObject InvertCameraObject;
    void Update() {
        if (Input.GetButtonDown("InvertColors")) {
            InvertCameraObject.SetActive(!InvertCameraObject.activeSelf);
        }
    }
}
