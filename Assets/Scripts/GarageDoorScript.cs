using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarageDoorScript : MonoBehaviour {
    public GameObject GarageDoor;
    public int Command = 0;

    public int Closed = 0;
    public int Opened = 30;

    public void FixedUpdate() {
        StartCoroutine(MoveDoorCoroutine());
    }

    public void OpenDoor() {
        Command = 1; 
    }

    IEnumerator MoveDoorCoroutine() {
        // Nothing to do, so let's do nothing
        if (Command == 0
            || (Command < 0 && GarageDoor.transform.position.y < Closed)
            || (Command > 0 && GarageDoor.transform.position.y > Opened)) {
            Command = 0;
            yield return new WaitForFixedUpdate();
        }

        // Move the door
        GarageDoor.transform.Translate(0f, (float)Command, 0f);

        yield return new WaitForFixedUpdate();
    }

    public void CloseDoor() {
        Command = -3;
    }
}
