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
        var meshFilter = road.AddComponent<MeshFilter>();

        // input data
        Vector3 pointCentre = new Vector3(0, 0, 0);

        //Vector3 point1 = new Vector3(-8, 0, -8);
        //Vector3 point2 = new Vector3(8, 0, -8);

        Vector3 point1 = new Vector3(-8, 0, 0);
        Vector3 point2 = new Vector3(8, 0, 0);



        // use mesh
        MeshGenerator meshGenerator = new MeshGenerator();
        mesh = meshGenerator.GetMesh(pointCentre, point1, point2);
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
