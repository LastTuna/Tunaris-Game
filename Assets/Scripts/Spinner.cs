using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour {
    public float rotSpeed = 1.0f;
    public Vector3 rotDir = new Vector3(0, 1, 0);

	void Update () {
        gameObject.transform.rotation *= Quaternion.AngleAxis(rotSpeed, rotDir);
    }
}
