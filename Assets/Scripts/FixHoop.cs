using UnityEngine;

public class FixHoop : MonoBehaviour {

    private void Update()
    {
        gameObject.transform.localEulerAngles += new Vector3(0, 0, 5);
    }

    private void OnTriggerEnter(Collider other)
    {
        FindObjectOfType<Madness>().FixHoop();
    }
}
