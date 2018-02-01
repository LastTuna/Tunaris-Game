using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DefaultFocus : MonoBehaviour {
    public GameObject DefaultFocusObject;

    void Start() {
        StartCoroutine(SetDefaultFocus());
    }

    IEnumerator SetDefaultFocus() {
        // Disable select sound
        DefaultFocusObject.GetComponent<Animator>().SetBool("PlaySound", false);

        // Set focus
        GameObject.Find("EventSystem").GetComponent<EventSystem>().SetSelectedGameObject(DefaultFocusObject);

        // Wait a frame for the state to be evaluated
        yield return new WaitForEndOfFrame();

        // Re-enable select sound
        DefaultFocusObject.GetComponent<Animator>().SetBool("PlaySound", true);

        yield return null;
    }
}
