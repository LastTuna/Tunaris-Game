using UnityEngine;
using System.Collections;

public class CarCamera : MonoBehaviour {
    public Transform car;
    public Transform interiorCam;
    public float distance = 7f;
    public float height = 3.5f;
    public float rotationDamping = 3.0f;
    public float heightDamping = 2.0f;
    public float zoomRatio = 0.5f;
    public float DefaultFOV = 60;
    private Vector3 rotationVector;
    private float reversingFactor = 1f;
    public bool debug = true;
    // Variable to store the selected camera
    public int chosenCamera = 0;

    // cameras supported
    // basically this should be the max chosenCamera so 1 for chase + interior cam
    public int supportedCameras = 3;

    private void Start()
    {
        Camera camera = GetComponent<Camera>();
        float[] distances = new float[32];
        distances[8] = 60;
        camera.layerCullDistances = distances;
    }



    void LateUpdate() {
        if (car != null) {
            switch (chosenCamera) {
                // 0, default, 3rd person cam
                case 0:
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

                    //boilerplate shit i just want old camera ok
                    if (!debug)
                    {
                        transform.LookAt(car.position + car.forward * 10 * reversingFactor);
                    }
                    else
                    {
                        transform.LookAt(car);
                    }

                    //car.Find("Cockpit").gameObject.SetActive(false);

                    break;
                // 1: cockpit cam
                case 1:
                    interiorCam = car.Find("Interior Cam");
                    transform.position = interiorCam.position;
                    transform.rotation = car.rotation;

                    //car.Find("Cockpit").gameObject.SetActive(true);
                    break;
                // 2: wheel debug camera right side
                case 2:
                    transform.position = car.position;
                    transform.position += car.right * 5;
                    transform.LookAt(car);
                    break;
                // 3: wheel debug camera left side
                case 3:
                    transform.position = car.position;
                    transform.position -= car.right * 5;
                    transform.LookAt(car);
                    break;
                default: break;
            }
        }
    }

    void Update() {
        if (Input.GetButtonDown("Camera")) {
            chosenCamera = (chosenCamera < supportedCameras) ? chosenCamera + 1 : 0;
        }
    }

    void FixedUpdate() {
        if (car != null) {
            Vector3 localVilocity = car.InverseTransformDirection(car.GetComponent<Rigidbody>().velocity);
            if (localVilocity.z < -0.5f) {
                reversingFactor = -1f;
                rotationVector.y = car.eulerAngles.y + 180;
            } else {
                reversingFactor = 1f;
                rotationVector.y = car.eulerAngles.y;
            }
            float acc = car.GetComponent<Rigidbody>().velocity.magnitude;
            GetComponent<Camera>().fieldOfView = DefaultFOV + acc * zoomRatio;
        }
    }
}