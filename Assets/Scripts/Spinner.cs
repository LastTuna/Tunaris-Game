using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour {
    public float rotSpeed = 20.0f;

	void Update () {
        gameObject.transform.Rotate(new Vector3(0, 1), rotSpeed * Time.deltaTime);
	}
}
