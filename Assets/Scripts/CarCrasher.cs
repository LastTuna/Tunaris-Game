
using UnityEngine;

public class CarCrasher : MonoBehaviour
{
    public Transform car;
    public float maxMag;//cars durability
    public int i;
    public bool crash;
    public float damage;
    
    //deform things
    public MeshFilter originalMesh;
    Mesh carMesh;
    Vector3[] modMesh;

    public bool carfixed;

    // Use this for initialization
    void Start()
    {
        carMesh = originalMesh.mesh;
        modMesh = carMesh.vertices;
    }

    public void CarDeform(Vector3 impactPoint, float radius)
    {
        i = 0;
        foreach (Vector3 e in modMesh)
        {
            if (Physics.Raycast(transform.TransformPoint(modMesh[i]), transform.TransformPoint(modMesh[i]) - impactPoint, radius))
            {
                modMesh[i] = new Vector3(
                    e.x + (-0.5f + Mathf.Clamp01(-car.GetComponent<Rigidbody>().velocity.x)) / 20,
                    e.y,
                    e.z + (-0.5f + Mathf.Clamp01(-car.GetComponent<Rigidbody>().velocity.z)) / 20);
            }
            i++;
        }
        carMesh.vertices = modMesh;
        damage += (car.GetComponent<Rigidbody>().velocity.x + car.GetComponent<Rigidbody>().velocity.y + car.GetComponent<Rigidbody>().velocity.z) / maxMag;
    }

    public void OnCollisionEnter(Collision collision)
    {
        
        crash = true;
        foreach (var contact in collision.contacts)
        {
            CarDeform(contact.point, maxMag / 10);
        }

    }
    public void OnCollisionExit(Collision collision)
    {
        crash = false;
    }
}