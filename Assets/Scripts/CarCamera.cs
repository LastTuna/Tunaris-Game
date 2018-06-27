using UnityEngine;
using System.Collections;

public class CarCamera : MonoBehaviour {
    public Transform car;
    public float distance = 6.4f;
    public float height = 1.4f;
    public float rotationDamping = 3.0f;
    public float heightDamping = 2.0f;
    public float zoomRatio = 0.5f;
    public float DefaultFOV = 60;
    private Vector3 rotationVector;

    // Variable to store the selected camera
    public int chosenCamera = 0;

    void LateUpdate() {
        if (car != null) {
            switch (chosenCamera) {
                // 0, default, 3rd person cam
                case 0: {
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

                        car.Find("Cockpit").gameObject.SetActive(false);
                    }
                    break;
                // 1: cockpit cam
                case 1: {
                        transform.position = car.position;
                        transform.position += Vector3.up * 0.5f;
                        transform.rotation = car.rotation;
                        
                        car.Find("Cockpit").gameObject.SetActive(true);
                    }
                    break;
                default: break;
            }
        }
    }

    void Update() {
        if (Input.GetButtonDown("Camera")) {
            chosenCamera = (chosenCamera < 1) ? chosenCamera + 1 : 0;
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
            GetComponent<Camera>().fieldOfView = DefaultFOV + acc * zoomRatio;
        }
    }
}