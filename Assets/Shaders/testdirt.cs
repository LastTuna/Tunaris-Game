using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testdirt : MonoBehaviour {
	public float dirtvalue = 0f;

	
	// Update is called once per frame
	void Update () {
		if (dirtvalue >= 0f && dirtvalue <= 1f) {
			GetComponent<Renderer>().material.SetFloat("_FortniteRange", dirtvalue);
		}
	}
}
