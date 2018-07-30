using UnityEngine;

public class Lightning : MonoBehaviour {

    public MeshFilter lightningRod;
    Vector3[] ogMesh;
    Vector3[] distorsion;
    int i = 0;

    // Use this for initialization
    void Start () {

        ogMesh = lightningRod.mesh.vertices;
        distorsion = lightningRod.mesh.vertices;

    }
	
	// Update is called once per frame
	void Update () {
        i = 0;
        foreach(Vector3 e in ogMesh)
        {
            distorsion[i] = new Vector3(
                ogMesh[i].x + Random.Range(0, 0.4f),
                ogMesh[i].y,
                ogMesh[i].z + Random.Range(0, 0.4f));
            i++;
        }
        lightningRod.mesh.vertices = distorsion;

	}
}
