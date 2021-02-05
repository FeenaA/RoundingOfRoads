using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demonstration : MonoBehaviour
{
    public Material material;

    public GameObject spherePrefab;

    private Mesh mesh;

    public GameObject clickedSphere;

    private MeshFilter roadMeshFilter;

    private SphereMovement _sphereCenter;
    private SphereMovement _sphere1;
    private SphereMovement _sphere2;

    void Awake()
    {
        // spheres
        GameObject sphereCentre = Instantiate(spherePrefab);
        sphereCentre.transform.position = new Vector3(-5f, 0f, 0f);
        _sphereCenter = sphereCentre.GetComponent<SphereMovement>();
        _sphereCenter.PositionChanged += OnSpherePositionChanged;

        GameObject spherePoint1 = Instantiate(spherePrefab);
        spherePoint1.transform.position = new Vector3(5f, 0f, -10f);
        _sphere1 = spherePoint1.GetComponent<SphereMovement>();
        _sphere1.PositionChanged += OnSpherePositionChanged;

        GameObject spherePoint2 = Instantiate(spherePrefab);
        spherePoint2.transform.position = new Vector3(10f, 0f, 5f);
        _sphere2 = spherePoint2.GetComponent<SphereMovement>();
        _sphere2.PositionChanged += OnSpherePositionChanged;

        // mesh
        GameObject road = new GameObject("MyRoad");
        roadMeshFilter = road.AddComponent<MeshFilter>();

        MeshGenerator meshGenerator = new MeshGenerator();
        mesh = meshGenerator.GetMesh(_sphereCenter.transform.position, _sphere1.transform.position, _sphere2.transform.position);
        roadMeshFilter.sharedMesh = mesh;

        var meshRenderer = road.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
    }

    private void OnSpherePositionChanged()
    {
        MeshGenerator meshGenerator = new MeshGenerator();
        mesh = meshGenerator.GetMesh(_sphereCenter.transform.position, _sphere1.transform.position, _sphere2.transform.position);
        roadMeshFilter.sharedMesh = mesh;
    }
}
