using UnityEngine;

public class FixHoop : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        FindObjectOfType<Madness>().FixHoop();
    }
}
