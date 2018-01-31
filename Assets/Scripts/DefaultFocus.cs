using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DefaultFocus : MonoBehaviour {
    public GameObject DefaultFocusObject;

    void Start() {
        GameObject.Find("EventSystem").GetComponent<EventSystem>().SetSelectedGameObject(DefaultFocusObject);
    }
}
