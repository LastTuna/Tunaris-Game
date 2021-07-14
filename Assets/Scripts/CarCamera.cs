using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR;

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

    public GameObject[] tvCams;//tv cam transform array
    public bool CheckingCamDistance = true;
    public int currentTVCam = 0;
    // cameras supported
    // basically this should be the max chosenCamera so 1 for chase + interior cam
    public int supportedCameras = 3;

    public bool isVR = false;
    private Transform actualCamera = null;
    private Vector3? startInteriorCamPos = null;

    private void Start() {
        Camera camera = GetComponent<Camera>();
        float[] distances = new float[32];
        distances[8] = 60;
        camera.layerCullDistances = distances;

        isVR = XRDevice.isPresent;
        Debug.Log("CarCamera::isVR " + isVR);
        if (isVR) {
            InputTracking.disablePositionalTracking = false;
            actualCamera = transform.parent.transform;
        } else {
            actualCamera = transform;
        }
        tvCams = new GameObject[10];
        int i = 0;
        while (true)
        {
            GameObject dolor = GameObject.Find("TVcam_" + i);
            if (dolor == null) break;
            tvCams[i] = dolor;
            i++;
        }

    }

    void LateUpdate() {
        if (car != null) {
            switch (chosenCamera) {
                // 0, default, 3rd person cam
                case 0:
                    float wantedAngel = rotationVector.y;
                    float wantedHeight = car.position.y + height;
                    float myAngel = actualCamera.eulerAngles.y;
                    float myHeight = actualCamera.position.y;
                    myAngel = Mathf.LerpAngle(myAngel, wantedAngel, rotationDamping * Time.deltaTime);
                    myHeight = Mathf.Lerp(myHeight, wantedHeight, heightDamping * Time.deltaTime);
                    Quaternion currentRotation = Quaternion.Euler(0, myAngel, 0);
                    actualCamera.position = car.position;
                    actualCamera.position -= currentRotation * Vector3.forward * distance;
                    actualCamera.position = new Vector3(actualCamera.position.x, myHeight, actualCamera.position.z);

                    //boilerplate shit i just want old camera ok
                    if (!debug)
                    {
                        actualCamera.LookAt(car.position + car.forward * 10 * reversingFactor);
                    }
                    else
                    {
                        actualCamera.LookAt(car);
                    }

                    //car.Find("Cockpit").gameObject.SetActive(false);

                    break;
                // 1: cockpit cam
                case 1:
                    if (!interiorCam) {
                        // init interior cam position
                        interiorCam = car.Find("Interior Cam");
                    } else {
                        actualCamera.position = interiorCam.position;
                        actualCamera.rotation = car.rotation;
                    }

                    //car.Find("Cockpit").gameObject.SetActive(true);
                    break;
                    //tv cam
                case 2:
                    if (CheckingCamDistance) StartCoroutine(RefreshCamDistance());

                    actualCamera.LookAt(car);
                    break;
                // 3: wheel debug camera left side
                case 3:
                    actualCamera.position = car.position;
                    actualCamera.position -= car.right * 5;
                    actualCamera.LookAt(car);
                    break;
                default: break;
            }
        }
    }

    //originally i made this as a ienumerator to run it as coroutine because
    //the og one was really inefficient, iterating through all of them to find the nearest cam
    //i think this can be changed into a int function and return the index, and throw the pos value
    //in the main case statement
    IEnumerator RefreshCamDistance()
    {
        CheckingCamDistance = false;
        float bomboloni = Vector3.Distance(tvCams[currentTVCam].transform.position, car.position);
        yield return new WaitForSeconds(0.2f);
        //check distance to next camera
        if (currentTVCam + 1 != tvCams.Length)
        {
            if (bomboloni > Vector3.Distance(tvCams[currentTVCam + 1].transform.position, car.position))
            {
                currentTVCam++;
            }
        }
        //check distance to previous camera
        if(currentTVCam != 0)
        {
            if (bomboloni > Vector3.Distance(tvCams[currentTVCam - 1].transform.position, car.position))
            {
                currentTVCam--;
            }
        }
        //loopback to the first camera in the index
        if(currentTVCam + 1 == tvCams.Length)
        {
            if (bomboloni > Vector3.Distance(tvCams[0].transform.position, car.position))
            {
                currentTVCam = 0;
            }
        }
        if (currentTVCam == 0)
        {
            if (bomboloni > Vector3.Distance(tvCams[tvCams.Length - 1].transform.position, car.position))
            {
                currentTVCam = 0;
            }
        }
        actualCamera.position = tvCams[currentTVCam].transform.position;
        CheckingCamDistance = true;
    }

    void Update() {
        if (Input.GetButtonDown("Camera")) {
            chosenCamera = (chosenCamera < supportedCameras) ? chosenCamera + 1 : 0;
        }
        if(isVR && Input.GetKeyDown("m")) {
            if (startInteriorCamPos == null) {
                startInteriorCamPos = interiorCam.position;
            } else {
                interiorCam.position = (Vector3) startInteriorCamPos;
            }
            interiorCam.position -= InputTracking.GetLocalPosition(XRNode.Head);
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

            if (!isVR) {
                float acc = car.GetComponent<Rigidbody>().velocity.magnitude;
                GetComponent<Camera>().fieldOfView = DefaultFOV + acc * zoomRatio;
            }
        }
    }
}