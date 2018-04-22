using UnityEngine;
using System.Collections;
using System;

public class PlaneMeshDeformation : MonoBehaviour
{
    Vector3[] newVertices;
    Vector2[] newUV;
    int[] newTriangles;
    public int i = 0;
    public Material newmaterial;
    public Vector3 roundthis;

    Mesh originalMesh;
    Vector3[] origVertices;

    void Start()
    {
        Mesh currentMesh = new Mesh();
        Mesh originalMesh = new Mesh();

        currentMesh = GetComponent<MeshFilter>().mesh;
        originalMesh = GetComponent<MeshFilter>().mesh;

        origVertices = originalMesh.vertices;

        newVertices = currentMesh.vertices;
        newUV = currentMesh.uv;
        newTriangles = currentMesh.triangles;
    }
    
    void Update()
    {
        Mesh currentMesh = new Mesh();
        currentMesh = GetComponent<MeshFilter>().mesh;
            currentMesh.Clear();
            while (i > 0)
            {
            roundthis = new Vector3(Mathf.Round(origVertices[i].x * 1000), Mathf.Round(origVertices[i].y * 1000), Mathf.Round(origVertices[i].z * 1000));

            newVertices[i] = new Vector3(roundthis.x/1000, roundthis.y/1000, roundthis.z/1000);
            i++;
            }

            currentMesh.vertices = newVertices;
            currentMesh.uv = newUV;
            currentMesh.triangles = newTriangles;
            currentMesh.RecalculateNormals();
            i = 0;
        



        //gameObject.GetComponent<Renderer>().material = newmaterial;
    }
}