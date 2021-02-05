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
    private readonly float roadWidth = 2f;

    /// <summary>
    /// ����� ������ ����������
    /// </summary>
    private readonly float radius = 1f;

    /// <summary>
    /// ���������� ������������� ����� �� ����������
    /// </summary>
    private int _count = 5;

    private Vector3[] _vertices;
    private int[] _triangles;

    private Vector3 roundnessPoint;
    private int currentIndex = 0;
    private Vector3 crossingPoint;
    private bool isCrossingPointValid;

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
        // �������� ����� �����������
        GetCrossingPoint();

        // �������� ������ ��� ������ � ��������
        AllocateMemory();
        
        // �������� ����� ����������
        GetRoundnessPoint();

        // --- �������� ����������� �������� - ��� ������ ����������� ����

        // ��������� ������� ������ � ��������
        FillVerticesTriangles();
    }

    /// <summary>
    /// ��������� ������� ������ � ��������
    /// </summary>
    private void FillVerticesTriangles()
    {
        int currentVertic = 0;

        // �������� ����� ������� ���� ������
        GetStraightVertices(_point1, false, ref currentVertic);
          
        // ���������� ������ ��������
        Vector3 vectorStart = roundnessPoint - crossingPoint;
        Vector3 mediana = _pointCentre - crossingPoint;
        float angleRoads = Vector3.Angle(vectorStart, mediana) * 2f;

        // ���������� �����������
        //float pseudoScalarProduct = mediana.x * vectorStart.z - mediana.z * vectorStart.x;
        float pseudoScalarProduct = mediana.z * vectorStart.x - mediana.x * vectorStart.z;
        int sign = (pseudoScalarProduct > 0) ? -1 : 1;
        float deltaAngle = sign * angleRoads / _count;

        float shift;
        Vector3 road1 = _point1 - _pointCentre;
        if (road1.x < 0)
        {
            if (road1.z < 0)
            {
                shift = -90f;
            }
            else
            {
                shift = 180f;
            }
        }
        else
        {
            if (road1.z < 0)
            {
                shift = -90f;
            }
            else
            {
                shift = 180f;
            }
        }

        float angleX = Vector3.Angle(Vector3.right, crossingPoint - roundnessPoint) + shift;

        //float angleX = Vector3.Angle(Vector3.right, crossingPoint - roundnessPoint) - 90f;//+


        // ��������� ����������� �������
        int currentCount = (angleX == 0) ? 0 : _count;
        for (int i = 0; i <= currentCount; i++)
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
    /// �������� ������ ��� ������ � ��������
    /// </summary>
    private void AllocateMemory()
    {
        int verticesCount;
        int guardCount;

        if (isCrossingPointValid)
        {
            // ������ ��� �����
            verticesCount = (4 + _count) * 2;
            guardCount = 2 + 1 + _count;
        }
        else
        {
            // ���� ��������
            verticesCount = 3 * 2;
            guardCount = 2;
        }

        _vertices = new Vector3[verticesCount];
        _triangles = new int[guardCount * 2 * 3];
    }

    /// <summary>
    /// �������� ����� ������ ����������
    /// </summary>
    private void GetRoundnessPoint()
    {
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
    private void GetCrossingPoint()
    {
        // ������� ����� ������� �� ����� �����
        Vector3 road1 = _point1 - _pointCentre; 
        Vector3 road2 = _point2 - _pointCentre;

        // ��������������� ������������ - �� ������� ������������
        float pseudoScalarProduct = road1.x * road2.z - road1.z * road2.x;

        bool isDirectionReversed = pseudoScalarProduct < 0 ;
        float width = roadWidth / 2f + radius;

        GetLine(_point1, _pointCentre, width, out var line1, isDirectionReversed);
        GetLine(_pointCentre, _point2, width, out var line2, isDirectionReversed);

        // swap roads
        if (isDirectionReversed)
        {
            SwapRoads(ref line1, ref line2);
        }

        float k1 = line1.k;
        float k2 = line2.k;

        // case of parallel lines
        if ( k1 == k2 )
        {
            Vector3 road = _pointCentre - _point1;
            Vector3 normal = new Vector3(road.z, 0, -road.x).normalized;
            crossingPoint = _pointCentre + normal * width;

            isCrossingPointValid = false;
            return;
        }

        // not parallel lines
        float b1 = line1.bb;
        float b2 = line2.bb;

        crossingPoint = new Vector3(
            (b2 - b1) / (k1 - k2), 
            0, 
            (k1 * b2 - k2 * b1) / (k1 - k2));

        isCrossingPointValid = true;
    }

    /// <summary>
    /// ������������� ������
    /// </summary>
    private void SwapRoads(ref Line line1, ref Line line2)
    { 
        Line tempLine = line1;
        line1 = line2;
        line2 = tempLine;

        Vector3 tempPoint = _point1;
        _point1 = _point2;
        _point2 = tempPoint;
    }

    /// <summary>
    /// �����  ������, ������������ ������ point1, point2
    /// </summary>
    /// <param name="point1">start</param>
    /// <param name="point2">end</param>
    /// <param name="width">distance</param>
    /// <param name="k"></param>
    /// <param name="b"></param>
    private Vector3 GetLine(Vector3 point1, Vector3 point2, float width, out Line line, bool isDirectionReversed)
    {
        Vector3 road = new Vector3(point2.x - point1.x, point2.y - point1.y, point2.z - point1.z);
        Vector3 normal = new Vector3(road.z, 0, -road.x).normalized;

        int sign = 1;
        if ( isDirectionReversed )
        {
            sign = -1;
        }

        Vector3 point3 = point1 + sign * normal * width;
        Vector3 point4 = point2 + sign * normal * width;

        line = new Line(point3, point4);

        return normal;
    }
}
