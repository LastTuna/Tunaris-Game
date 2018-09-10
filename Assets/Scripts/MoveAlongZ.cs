using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAlongZ : MonoBehaviour {
	// Update is called once per frame
	void Update () {
        this.gameObject.transform.Translate(0, 0, 0.125f);
	}
}
