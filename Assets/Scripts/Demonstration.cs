using UnityEngine;

public class Demonstration : MonoBehaviour
{
    public Material material;
    public Material materialSphere1;
    public Material materialSphere2; 

    public GameObject spherePrefab;

    private Mesh mesh;
    private MeshFilter roadMeshFilter;

    public GameObject clickedSphere;

    private SphereMovement _sphere1;
    private SphereMovement _sphere2;
    private SphereMovement _sphere3;
    private SphereMovement _sphere4;
    private SphereMovement _sphere5;

    /// <summary>
    /// ширина дорог
    /// </summary>
    private readonly float roadWidth = 2f;

    void Awake()
    {
        int currentPoint = 0;

        Vector3[] points = new Vector3[]
    {
            new Vector3(-5f, 0f, -1f),
            new Vector3(5f, 0f, -10f),
            new Vector3(13f, 0f, 3f),
            new Vector3(14f, 0f, 3f),
            new Vector3(5f, 0f, -1f),
    };

        GameObject spherePoint1 = Instantiate(spherePrefab);
        spherePoint1.transform.position = points[currentPoint];
        _sphere1 = spherePoint1.GetComponent<SphereMovement>();
        _sphere1.PositionChanged += OnSpherePositionChanged;
        _sphere1.GetComponent<MeshRenderer>().material = materialSphere1;

        GameObject spherePoint2 = Instantiate(spherePrefab);
        spherePoint2.transform.position = points[++currentPoint];
        _sphere2 = spherePoint2.GetComponent<SphereMovement>();
        _sphere2.PositionChanged += OnSpherePositionChanged;

        GameObject spherePoint3 = Instantiate(spherePrefab);
        spherePoint3.transform.position = points[++currentPoint];
        _sphere3 = spherePoint3.GetComponent<SphereMovement>();
        _sphere3.PositionChanged += OnSpherePositionChanged;

        GameObject spherePoint4 = Instantiate(spherePrefab);
        spherePoint4.transform.position = points[++currentPoint];
        _sphere4 = spherePoint4.GetComponent<SphereMovement>();
        _sphere4.PositionChanged += OnSpherePositionChanged;

        GameObject spherePoint5 = Instantiate(spherePrefab);
        spherePoint5.transform.position = points[++currentPoint];
        _sphere5 = spherePoint5.GetComponent<SphereMovement>();
        _sphere5.PositionChanged += OnSpherePositionChanged;
        _sphere5.GetComponent<MeshRenderer>().material = materialSphere2;

        // mesh
        GameObject road = new GameObject("MyRoad");
        roadMeshFilter = road.AddComponent<MeshFilter>();

        RoadGenerator roadGenerator = new RoadGenerator();
        mesh = roadGenerator.GetMesh(points, roadWidth); 
        roadMeshFilter.sharedMesh = mesh;

        var meshRenderer = road.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
    }

    private void OnSpherePositionChanged()
    {
        Vector3[] points = new Vector3[]
        {
            _sphere1.transform.position,
            _sphere2.transform.position,
            _sphere3.transform.position,
            _sphere4.transform.position,
            _sphere5.transform.position,
        };

        RoadGenerator meshGenerator = new RoadGenerator();
        mesh = meshGenerator.GetMesh(points, roadWidth);
        roadMeshFilter.sharedMesh = mesh;
    }
}
