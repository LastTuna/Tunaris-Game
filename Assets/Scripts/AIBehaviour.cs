using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBehaviour : MonoBehaviour {
    public CarBehaviour car;
    public CourseController courseController;

    private int currentWaypointIndex = 0;

    private Rigidbody rb;

    void Start() {
        //car.manual = 0;
        car.gear = 5;
        rb = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void FixedUpdate() {
        AIWaypointBehaviour next = courseController.AIWaypoints[currentWaypointIndex].GetComponent<AIWaypointBehaviour>();
        Vector3 toNextWP = courseController.AIWaypoints[currentWaypointIndex].transform.position - gameObject.transform.position;
        toNextWP.y = 0;

        // steering calculations
        float angleraw = Vector3.SignedAngle(gameObject.transform.forward, toNextWP, Vector3.up);
        float angle = angleraw * (toNextWP.magnitude < next.DampingDistance ? toNextWP.magnitude / next.DampingDistance : 1);
        float steerpos = CustomInput.GetAxis("Steering");

        // neutral zone didn't work, reset steering when changing direction
        if(steerpos < 0 && angle > 0 || steerpos > 0 && angle < 0) {
            steerpos = 0;
        }
        
        if (angle > 5) {
            //Debug.Log("A:" + angle + ";SP:" + (steerpos < 1f ? steerpos + Mathf.Clamp(rb.velocity.magnitude * 0.001f, 0, 0.01f) : steerpos));
            CustomInput.SetAxis("Steering", steerpos < 1f ? steerpos + Mathf.Clamp(rb.velocity.magnitude * 0.001f, 0, 0.01f) : steerpos);
        } else if (angle < -5) {
            //Debug.Log("A:" + angle + ";SP:" + (steerpos > -1f ? steerpos - Mathf.Clamp(rb.velocity.magnitude * 0.001f, 0, 0.01f) : steerpos));
            CustomInput.SetAxis("Steering", steerpos > -1f ? steerpos - Mathf.Clamp(rb.velocity.magnitude * 0.001f, 0, 0.01f) : steerpos);
        } else {
            CustomInput.SetAxis("Steering", 0f);
        }

        // throttle and brake calculations
        if (next.WaypointType == AIWaypointType.NORMAL) {
            CustomInput.SetAxis("Throttle", 0.5f - Mathf.Abs(CustomInput.GetAxis("Steering")) * Mathf.Clamp(10f - rb.velocity.magnitude, 0, 10f));
            CustomInput.SetAxis("Brake", 0f);
        } else if(next.WaypointType == AIWaypointType.BRAKE) {
            CustomInput.SetAxis("Throttle", 0f);
            float brake = CustomInput.GetAxis("Brake");
            CustomInput.SetAxis("Brake", brake < 1f ? brake + 0.01f:brake);
        }

        if (toNextWP.magnitude < 10f) {
            if (currentWaypointIndex < courseController.AIWaypoints.Length-1) {
                currentWaypointIndex++;
            } else {
                currentWaypointIndex = 0;
            }
        }
    }
}