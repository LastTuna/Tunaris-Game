
using UnityEngine;

public class CarCrasher : MonoBehaviour
{
    public Transform car;
    public float maxMag;//cars durability
    public int i;
    public bool crash;
    public float damage;

    public bool madness = false;//toggle sim/MADNESS damage
    
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

    public void CarDeform(Vector3 impactPoint)
    {
        i = 0;
        Vector3 dolor = transform.InverseTransformPoint(impactPoint);
        //dolor lol get it the collision point HAHAHAHA
        //cache the impact point localized into car's local position
        Vector3 carspeed = car.GetComponent<Rigidbody>().velocity;
        float speedfactor = maxMag * (Mathf.Abs(carspeed.x) +
            Mathf.Abs(carspeed.y) +
            Mathf.Abs(carspeed.z)) / 3;
        Debug.Log(speedfactor);
        foreach (Vector3 e in modMesh)
        {
            //sift through all the verts and if its near enough(dictated by maxmagnitude) deform
            if (Mathf.Abs((e.x - dolor.x) + (e.y - dolor.y) + (e.z - dolor.z)) < maxMag)
            {
                if (madness){
                    e.Scale(new Vector3(1 - speedfactor, 1 - speedfactor, 1 - speedfactor));
                    //madness damage
                } else {
                    e.Scale(new Vector3(Random.Range(0.95f, 1f), 1f, Random.Range(0.95f, 1f)));
                    //sim dmg
                }
                modMesh[i] = e;
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
            CarDeform(contact.point);
        }

    }
    public void OnCollisionExit(Collision collision)
    {
        crash = false;
    }
}