using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demonstration : MonoBehaviour
{
    public Material material;

    void Start()
    {
        GameObject road = new GameObject("MyRoad");

        // MeshFilter 
        var meshFilter = road.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = new Mesh();

        // use mesh
        MeshGenerator meshGenerator = new MeshGenerator();
        meshFilter.sharedMesh = meshGenerator.GetMesh();

        // MeshRenderer
        var meshRenderer = road.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
    }
}
