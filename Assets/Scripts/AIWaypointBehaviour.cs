using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIWaypointType {
    NORMAL,
    BRAKE
}

public class AIWaypointBehaviour : MonoBehaviour {
    public AIWaypointType WaypointType;
    public float DampingDistance = 100f;
}
