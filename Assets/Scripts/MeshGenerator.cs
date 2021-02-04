using UnityEngine;

public class MeshGenerator
{
    // ������� �����
    private Vector3 _pointCentre;
    private Vector3 _point1;
    private Vector3 _point2;

    /// <summary>
    /// ������ �����
    /// </summary>
    private readonly float roadWidth = 3f;

    /// <summary>
    /// ����� ������ ����������
    /// </summary>
    private readonly float radius = 1f;

    /// <summary>
    /// ���������� ������������� ����� �� ����������
    /// </summary>
    private readonly int count = 5;

    private Vector3[] _vertices;
    private int[] _triangles;

    private Vector3 roundnessPoint;
    private int currentIndex = 0;
    private Vector3 crossingPoint;

    /// <summary>
    /// ������� � ��������� mesh
    /// </summary>
    public Mesh GetMesh( Vector3 pointCentre, Vector3 point1, Vector3 point2)
    {
        _pointCentre = pointCentre;
        _point1 = point1;
        _point2 = point2;

        GenerateVerticesTriangles();

        Mesh mesh = new Mesh
        {
            vertices = _vertices,
            triangles = _triangles
        };

        mesh.RecalculateNormals();
        return mesh;
    }

    /// <summary>
    /// ��������� ������ � ��������
    /// </summary>
    private void GenerateVerticesTriangles()
    {
        _vertices = new Vector3[(4 + count) * 2];

        int guardCount = 2 + 1 + count;
        _triangles = new int[guardCount * 2 * 3];

        // �������� ����� �����������
        float width = roadWidth / 2f + radius;
        GetLine(_point1, _pointCentre, width, out var line1);
        GetLine(_pointCentre, _point2, width, out var line2);
        crossingPoint = GetCrossingPoint(line1, line2);

        // �������� ����� ����������
        GetRoundedPoints();

        // --- �������� ����������� ��������

        int currentVertic = 0;

        // �������� ����� ������� ���� ������
        GetStraightVertices(_point1, false, ref currentVertic);

        float angleRoads = Vector3.Angle(crossingPoint - roundnessPoint, crossingPoint - _pointCentre) * 2f;
        float deltaAngle = angleRoads / count;

        //float angleX = Vector3.Angle(Vector3.left, crossingPoint - roundnessPoint) - 180f;
        float angleX = Vector3.Angle(Vector3.right, crossingPoint - roundnessPoint) - 90f;

        // ��������� ����������� �������
        for (int i = 0; i <= count; i++)
        {
            _vertices[currentVertic++] = Rotate(radius, angleX + i * deltaAngle, crossingPoint);
            _vertices[currentVertic++] = Rotate(radius + roadWidth, angleX + i * deltaAngle, crossingPoint);

            GetGuard(currentVertic - 4, currentVertic - 3, currentVertic - 2, currentVertic - 1);
        }
        // �������� ����� ������� ���� ������
        GetStraightVertices(_point2, true, ref currentVertic);

        GetGuard(currentVertic - 4, currentVertic - 3, currentVertic - 2, currentVertic - 1);
    }

    /// <summary>
    /// �������� ����� ������ ����������
    /// </summary>
    private void GetRoundedPoints()
    {
        // ������� ��� ������� ����
        Vector3 road = _point1 - _pointCentre;
        Vector3 normal = new Vector3(road.z, 0, -road.x).normalized;
        roundnessPoint = crossingPoint + normal * radius;
    }

    /// <summary>
    /// ������� �� ������ �������� �����
    /// </summary>
    /// <param name="point">������� �� ���� ������</param>
    /// <param name="currentNum">������� ���������� ������</param>
    /// <param name="roundnessPoint">������ ���������� �� ��������� �������</param>
    /// <param name="RoundnessPoint">������ ���������� �� ������� �������</param>
    private void GetStraightVertices(Vector3 point, bool isInverted, ref int currentVertic)
    {
        float halfRoadWidth = roadWidth / 2f;

        Vector3 road = point - _pointCentre;
        Vector3 normal = new Vector3(road.z, 0, -road.x).normalized * halfRoadWidth;

        Vector3 vertic1 = point - normal;
        Vector3 vertic2 = point + normal;

        if (!isInverted)
        {
            _vertices[currentVertic++] = vertic1;
            _vertices[currentVertic++] = vertic2;
        }
        else
        {
            _vertices[currentVertic++] = vertic2;
            _vertices[currentVertic++] = vertic1;
        }
    }

    /// <summary>
    /// ������� �������
    /// </summary>
    /// <param name="radius">������ ��������</param>
    /// <param name="angle">���� ��������</param>
    /// <param name="center">����� ��������</param>
    /// <returns></returns>
    private static Vector3 Rotate(float radius, float angle, Vector3 center)
    {
        Vector3 point = new Vector3
        {
            x = radius * Mathf.Sin(angle * Mathf.Deg2Rad),
            z = radius * Mathf.Cos(angle * Mathf.Deg2Rad)
        };

        return center + point;
    }

    /// <summary>
    /// ���������� �������� ��� �������������
    /// </summary>
    /// <param name="currentIndex"></param>
    /// <param name="v00">������ ����� ������ �������</param>
    /// <param name="v01">������ ����� ������� �������</param>
    /// <param name="v10">������ ������ ������ �������</param>
    /// <param name="v11">������ ������ ������� �������</param>
    private void GetGuard(int v00, int v01, int v10, int v11)
    {
        _triangles[currentIndex++] = v00;
        _triangles[currentIndex++] = v01;
        _triangles[currentIndex++] = v11;

        _triangles[currentIndex++] = v00;
        _triangles[currentIndex++] = v11;
        _triangles[currentIndex++] = v10;
    }

    /// <summary>
    /// ����� ����� ����������� ������ 1) y = a*x + c, 2) y = b*x + d
    /// </summary>
    /// <param name="a"></param>
    /// <param name="c"></param>
    /// <param name="b"></param>
    /// <param name="d"></param>
    /// <returns>����� �����������</returns>
    private Vector3 GetCrossingPoint(Line line1, Line line2)
    {
        float k1 = line1.k;
        float b1 = line1.bb;
        float k2 = line2.k;
        float b2 = line2.bb;

        return new Vector3((b2 - b1) / (k1 - k2), 0, (k1 * b2 - k2 * b1) / (k1 - k2));

        /*if (line1.a == 0f)
        {
            var temp = line1;
            line1 = line2;
            line2 = temp;
        }

        var res = new Vector3
        {
            z = (line1.a * line2.c - line2.a * line1.c) / (line1.a * line2.b - line2.a * line1.b),
        };

        res.x = (line1.c - res.y * line1.b) / line1.a;

        return res;*/
    }

    /// <summary>
    /// �����  ������, ������������ ������ point1, point2
    /// </summary>
    /// <param name="point1">start</param>
    /// <param name="point2">end</param>
    /// <param name="width">distance</param>
    /// <param name="k"></param>
    /// <param name="b"></param>
    private Vector3 GetLine(Vector3 point1, Vector3 point2, float width, out Line line)
    {
        Vector3 road = new Vector3(point2.x - point1.x, point2.y - point1.y, point2.z - point1.z);
        Vector3 normal = new Vector3(road.z, 0, -road.x).normalized;

        Vector3 point3 = point1 + normal * width;
        Vector3 point4 = point2 + normal * width;

        line = new Line(point3, point4);

        return normal;
    }
}
