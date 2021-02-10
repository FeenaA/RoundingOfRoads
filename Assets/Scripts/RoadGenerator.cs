using System;
using UnityEngine;

public class RoadGenerator
{
    // ������� �����
    private Vector3 _pointCentre;
    private Vector3 _point1;
    private Vector3 _point2;

    /// <summary>
    /// ������ �����
    /// </summary>
    private readonly float roadWidth = 2f;
    private float halfWidth;

    /// <summary>
    /// ����� ������ ����������
    /// </summary>
    private readonly float radius = 1f;

    /// <summary>
    /// ���������� ������������� ����� �� ����������
    /// </summary>
    private readonly int _count = 5;

    private Vector3[] _vertices;
    private int[] _triangles;
    private Vector3[] _normals;

    private Vector3 roundnessPoint;
    private int currentIndex = 0;
    private Vector3 crossingPoint;
    private bool isCrossingPointValid;

    private float angle;
    private Quaternion rotation;

    /// <summary>
    /// ������� � ��������� mesh
    /// </summary>
    public Mesh GetMesh(Vector3[] points)
    {
        halfWidth = roadWidth / 2f;


        // ---
        // ���� ������ ��������
        // ��������� ��������� ��� ����� 1_0, 2_0
//        _vertices[currentIndex] = new Vector3(-halfWidth, 0f, 0f);
//        _vertices[++currentIndex] = new Vector3(-halfWidth, 0f, 0f);

        // � �����:
        //      ��������� ������ ������ � ����� ��

        //      ����� 1_0, 2_0 � ����� ��
        //      mesh: ������ ������������� � ��������
        //      � ������ ��
        //      ��������� ����� point1 � ����� ��������

        // ��������� ��������� ��� �����
        // ��������� ��������� �������������
        // ---


        int iterationCount = points.Length - 2;

        // the first corner
        _point1 = points[0];
        _pointCentre = points[1];
        _point2 = points[2];

        ChangeSystemOld2New(ref _point1, ref _pointCentre, ref _point2);
        GenerateVerticesTriangles();
        ChangeSystemNew2Old(points[0]);

        // middle corners
        for (int i = 1; i < iterationCount; i++)
        {
            _point1 = points[i];
            _pointCentre = points[i + 1];
            _point2 = points[i + 2];

            ChangeSystemOld2New(ref _point1, ref _pointCentre, ref _point2);
            /*
                        GenerateVerticesTriangles();
                        ChangeSystemNew2Old(points[i - 1]);
            */
        }

        Mesh mesh = new Mesh
        {
            vertices = _vertices,
            triangles = _triangles,
            normals = _normals,
        };

        return mesh;
    }

    /// <summary>
    /// change initial system to temporary 
    /// </summary>
    //private void ChangeSystemOld2New()
    private void ChangeSystemOld2New(ref Vector3 _point1, ref Vector3 _pointCentre, ref Vector3 _point2)
    {
        // change an origin point
        _pointCentre -= _point1;
        _point2 -= _point1;
        _point1 = Vector3.zero; 

        // get point direction relative to pivot
        Vector3 dir = _pointCentre - _point1;
        var s = -1 * Mathf.Sign(_pointCentre.x);
        angle = s * Vector3.Angle(dir, Vector3.forward);

        // rotation
        rotation = Quaternion.AngleAxis(angle, Vector3.up);

        // calculate rotated points
        _pointCentre = rotation * _pointCentre;
        _point2 = rotation * _point2;
    }

    /// <summary>
    /// move _vertices to old basis
    /// </summary>
    /// <param name="point1">pivot</param>
    private void ChangeSystemNew2Old(Vector3 point1)
    {
        // rotation
        var rot = Quaternion.Inverse(rotation);

        // move and rotate vertices
        for (int i = 0; i < _vertices.Length; i++)
        {
            _vertices[i] = rot * _vertices[i] + point1;
            _normals[i] = Vector3.up;
        }
    }

    /// <summary>
    /// ��������� ������ � ��������
    /// </summary>
    private void GenerateVerticesTriangles()
    {
        // �������� ����� �����������
        CalculateCenterOfCircle();

        // �������� ������ ��� ������ � ��������
        AllocateMemory();

        // �������� ����� ����������
        GetRoundnessPoint();

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
        GetStraightVertices(_point1, 1, ref currentVertic);

        // ���������� ������ ��������
        Vector3 vectorStart = roundnessPoint - crossingPoint;
        Vector3 mediana = _pointCentre - crossingPoint;
        float angleRoads = Vector3.Angle(vectorStart, mediana) * 2f;

        float deltaAngle = angleRoads / _count;
        float angleX = Vector3.Angle(Vector3.right, crossingPoint - roundnessPoint) - 90f;

        // ��������� ����������� �������
        int currentCount = (angleX == 0) ? 0 : _count;
        for (int i = 0; i <= currentCount; i++)
        {
            _vertices[currentVertic++] = Rotate(radius, angleX + i * deltaAngle, crossingPoint);
            _vertices[currentVertic++] = Rotate(radius + roadWidth, angleX + i * deltaAngle, crossingPoint);

            GetGuard(currentVertic - 4);
        }
        // �������� ����� ������� ���� ������
        GetStraightVertices(_point2, -1, ref currentVertic);

        GetGuard(currentVertic - 4);
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
        _normals = new Vector3[verticesCount];
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
    /// <param name="point">����� ������ ��� ������ ������</param>
    /// <param name="isInverted">������ ��� �������� �������</param>
    /// <param name="currentVertic">������� ���������� ������</param>
    private void GetStraightVertices(Vector3 point, int isInverted, ref int currentVertic)
    {
        Vector3 road = point - _pointCentre;
        Vector3 normal = new Vector3(road.z, 0, -road.x).normalized * halfWidth;

        Vector3 vertic1 = point - isInverted * normal;
        Vector3 vertic2 = point + isInverted * normal;

        _vertices[currentVertic++] = vertic1;
        _vertices[currentVertic++] = vertic2;
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
    /// <param name="index">������ ����� ������ �������</param>
    /// <param name="v01">������ ����� ������� �������</param>
    /// <param name="v10">������ ������ ������ �������</param>
    /// <param name="v11">������ ������ ������� �������</param>
    private void GetGuard(int index)
    {
        _triangles[currentIndex++] = index;
        _triangles[currentIndex++] = index + 1;
        _triangles[currentIndex++] = index + 3;

        _triangles[currentIndex++] = index;
        _triangles[currentIndex++] = index + 3;
        _triangles[currentIndex++] = index + 2;
    }

    /// <summary>
    /// ����� ����� �����������
    /// </summary> 
    private void CalculateCenterOfCircle()
    {
        int directionSign = DetectDirection();

        float width = halfWidth + radius;
        GetLine(_point1, _pointCentre, width, out var line1, directionSign);
        GetLine(_pointCentre, _point2, width, out var line2, directionSign);

        // swap roads
        if (directionSign < 0)
        {
            SwapRoads(ref line1, ref line2);
        }

        // get parametres of lines 
        float a1 = line1.a;
        float b1 = line1.b;
        float c1 = line1.c;

        float a2 = line2.a;
        float b2 = line2.b;
        float c2 = line2.c;

        // case of parallel lines
        float denominator = a1 * b2 - a2 * b1;
        if (denominator == 0f)
        {
            Vector3 road = _pointCentre - _point1;
            Vector3 normal = new Vector3(road.z, 0, -road.x).normalized;
            crossingPoint = _pointCentre + normal * width;

            isCrossingPointValid = false;
            return;
        }

        // not parallel lines
        crossingPoint = new Vector3(
            (c2 * b1 - c1 * b2) / denominator,
            0,
            (a2 * c1 - a1 * c2) / denominator
            );

        isCrossingPointValid = true;
    }

    /// <summary>
    /// ����������, �������� �� ���� �����
    /// </summary>
    /// <returns></returns>
    private int DetectDirection()
    {
        // ������� ����� ������� �� ����� �����
        Vector3 road1 = _point1 - _pointCentre;
        Vector3 road2 = _point2 - _pointCentre;

        // ��������������� ������������ - �� ������� ������������
        float pseudoScalarProduct = road1.x * road2.z - road1.z * road2.x;

        return Math.Sign(pseudoScalarProduct);
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
    /// <param name="width">���������� ����� ��������</param>
    /// <param name="line"></param>
    /// <param name="isDirectionReversed"></param>
    /// <returns></returns>
    private Vector3 GetLine(Vector3 point1, Vector3 point2, float width, out Line line, int sign)
    {
        Vector3 road = new Vector3(point2.x - point1.x, point2.y - point1.y, point2.z - point1.z);
        Vector3 normal = new Vector3(road.z, 0, -road.x).normalized;

        Vector3 point3 = point1 + sign * normal * width;
        Vector3 point4 = point2 + sign * normal * width;

        line = new Line(point3, point4);

        return normal;
    }
}
