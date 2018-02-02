using UnityEngine;
using System.Collections;

public class CarCamera : MonoBehaviour {
    public Transform car;
    public float distance = 6.4f;
    public float height = 1.4f;
    public float rotationDamping = 3.0f;
    public float heightDamping = 2.0f;
    public float zoomRacio = 0.5f;
    public float DefaultFOV = 60;
    private Vector3 rotationVector;

    void LateUpdate() {
        if (car != null) {
            float wantedAngel = rotationVector.y;
            float wantedHeight = car.position.y + height;
            float myAngel = transform.eulerAngles.y;
            float myHeight = transform.position.y;
            myAngel = Mathf.LerpAngle(myAngel, wantedAngel, rotationDamping * Time.deltaTime);
            myHeight = Mathf.Lerp(myHeight, wantedHeight, heightDamping * Time.deltaTime);
            Quaternion currentRotation = Quaternion.Euler(0, myAngel, 0);
            transform.position = car.position;
            transform.position -= currentRotation * Vector3.forward * distance;
            transform.position = new Vector3(transform.position.x, myHeight, transform.position.z);
            transform.LookAt(car);
        }
    }

    void FixedUpdate() {
        if (car != null) {
            Vector3 localVilocity = car.InverseTransformDirection(car.GetComponent<Rigidbody>().velocity);
            if (localVilocity.z < -0.5f) {
                rotationVector.y = car.eulerAngles.y + 180;
            } else {
                rotationVector.y = car.eulerAngles.y;
            }
            float acc = car.GetComponent<Rigidbody>().velocity.magnitude;
            GetComponent<Camera>().fieldOfView = DefaultFOV + acc * zoomRacio;
        }
    }
}