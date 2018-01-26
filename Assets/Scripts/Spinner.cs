using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour {
	void Update () {
        gameObject.transform.Rotate(new Vector3(0, 1), 20 * Time.deltaTime);
	}
}
