using UnityEngine;
using System.Collections;

public class CarCameraBusrider : MonoBehaviour {
    public Transform car;
    public Transform interiorCam;

    // Variable to store the selected camera
    public int chosenCamera = 0;

    void LateUpdate() {
        if (car != null) {
            switch (chosenCamera) {
                // 0, default, 3rd person cam
                case 0:
                    interiorCam = car.Find("Busrider Cam");
                    transform.position = interiorCam.position;
                    transform.rotation = car.rotation;
                    break;
                default: break;
            }
        }
    }
}