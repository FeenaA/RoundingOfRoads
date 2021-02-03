using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demonstration : MonoBehaviour
{
    public Material material;

    private Mesh mesh;

    void Start()
    {
        GameObject road = new GameObject("MyRoad");

        // MeshFilter 
        var meshFilter = road.AddComponent<MeshFilter>();
        //meshFilter.sharedMesh = new Mesh();

        // use mesh
        MeshGenerator meshGenerator = new MeshGenerator();
        mesh = meshGenerator.GetMesh();
        meshFilter.sharedMesh = mesh;

        // MeshRenderer
        var meshRenderer = road.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
    }

    /*private void OnDrawGizmos()
    {
        if (mesh != null)
        {
            for (int i = 4; i < mesh.vertices.Length - 4; ++i)
            {
                Gizmos.DrawSphere(mesh.vertices[i], 0.5f);
            }
        }
    }*/
}
