using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour {
    public float rotSpeed = 1.0f;

	void Update () {
        gameObject.transform.rotation *= Quaternion.AngleAxis(rotSpeed, new Vector3(0, 1, 0));
    }
}
