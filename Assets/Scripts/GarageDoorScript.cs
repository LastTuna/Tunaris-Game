using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarageDoorScript : MonoBehaviour {
    public GameObject GarageDoor;
    public int Command = 0;

    public int Closed = -30;
    public int Opened = -4;

    public void Start() {
        StartCoroutine(MoveDoorCoroutine());
    }

    public void OpenDoor() {
        Command = 1;
    }

    IEnumerator MoveDoorCoroutine() {
        // Nothing to do, so let's do nothing
        if (Command == 0
            || GarageDoor.transform.position.y < Closed
            || GarageDoor.transform.position.y > Opened) {
            yield return new WaitForEndOfFrame();
        }

        // Move the door
        GarageDoor.transform.Translate(0f, (float)Command, 0f);

        yield return new WaitForFixedUpdate();
    }

    public void CloseDoor() {
        Command = -1;
    }
}
