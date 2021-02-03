using UnityEngine;

public class MeshGenerator 
{
    // input data
    Vector3 pointCentre = new Vector3(0, 0, 0);
    Vector3 point1 = new Vector3(-5, 0, -5);
    Vector3 point2 = new Vector3(5, 0, -5);

    // ������ �����
    // --- ������������� ������ ������ ��������
    private readonly float roadWidth = 5f;

    // ����� ������ ����������
    // --- ������ ������������
    private readonly float radius = 3f;
    // --- ���������� ������������� ������������
    private readonly int count = 5;

    private Vector3[] _vertices;
    private int[] _triangles;
     
    private Vector3 roundnessPoint1;
    private Vector3 roundnessPoint2;
    private Vector3 RoundnessPoint1;
    private Vector3 RoundnessPoint2;

    /// <summary>
    /// ������� � ��������� mesh
    /// </summary>
    public Mesh GetMesh()
    {
        CalculateSize();

        Mesh mesh = new Mesh
        {
            vertices = _vertices,
            triangles = _triangles
        };

        mesh.RecalculateNormals();
        return mesh;
    }

    /// <summary>
    /// ������� ...
    /// </summary>
    private void CalculateSize()
    {
        _vertices = GenerateVertices();
        _triangles = GenerateTriangles();
    }

    private Vector3[] GenerateVertices()
    {
        _vertices = new Vector3[ (4 + count) * 2];

        int currentNum = 0;
        GetStraightSection(point1, ref currentNum, ref roundnessPoint1, ref RoundnessPoint1);
        GetRoundedSection(ref currentNum);
        GetStraightSection(point2, ref currentNum, ref roundnessPoint2, ref RoundnessPoint2);

        return _vertices;
    }

    /// <summary>
    /// ���������� ������� �������� ��� �������������
    /// </summary>
    /// <returns>_triangles - ������ ��������</returns>
    private int[] GenerateTriangles()
    { 
        int guardCount = 2 + 1 + count;
        _triangles = new int[guardCount * 2 * 3 ];

        int currentIndex = 0;
        int v = 0;
        for (int i = 0; i < guardCount; i++)
        {
            GetGuard(ref currentIndex, v, v + 1, v + 2, v + 3);
            v += 2;
        }

        return _triangles;
    }

    /// <summary>
    /// ������� �� ������ �������� �����
    /// </summary>
    /// <param name="point">������� �� ���� ������</param>
    /// <param name="currentNum">������� ���������� ������</param>
    /// <param name="roundnessPoint">������ ���������� �� ��������� �������</param>
    /// <param name="RoundnessPoint">������ ���������� �� ������� �������</param>
    private void GetStraightSection(Vector3 point, ref int currentNum, ref Vector3 roundnessPoint, ref Vector3 RoundnessPoint)
    {
        float halfRoadWidth = roadWidth / 2f;

        Vector3 road = new Vector3(point.x - pointCentre.x, point.y - pointCentre.y, point.z - pointCentre.z);
        Vector3 normal = new Vector3(road.z, 0, -road.x).normalized * halfRoadWidth;

        _vertices[currentNum++] = point - normal;
        _vertices[currentNum++] = point + normal;

        float fullLength = Vector3.Distance(point, pointCentre);
        Vector3 tempPoint = pointCentre + (point - pointCentre) * (halfRoadWidth + radius) / fullLength;

        roundnessPoint = tempPoint - normal;
        RoundnessPoint = tempPoint + normal;

        _vertices[currentNum++] = roundnessPoint;
        _vertices[currentNum++] = RoundnessPoint;
    }



    private void GetRoundedSection(ref int currentNum)
    {
        Line line1 = new Line();
        Line line2 = new Line();

        float roadWidthHalf = roadWidth / 2f;

        // --- �������� ����������� ��������
        float width = roadWidthHalf + radius;
        GetLine(point1, pointCentre, width, ref line1);
        GetLine(pointCentre, point2, width, ref line2);

        Vector3 crossingPoint = GetCrossingPoint(line1, line2);
         
        float angleRoads = Vector3.Angle(point1 - pointCentre, pointCentre - point2);
        float deltaAngle = (180f - angleRoads) / (count + 1);


        Line line = new Line(crossingPoint, roundnessPoint1);

        for (int i = 0; i < count; i++)
        {
            line.Rotate(deltaAngle);

            // ������� ����� �� ����� ������� � �������� � � ������ ������

            // ������� ����� �� ������� ������� � �������� � � ������ ������

            //_vertices[currentNum++] = point - normal;
            int x = 5;

        }
    }

    /*Line line3 = new Line();
Line line4 = new Line();
GetLine(point1, pointCentre, roadWidthHalf, ref line3);
GetLine(pointCentre, point2, roadWidthHalf, ref line4);

// �������������� �� centralPoint � ������ ����� ������
Line line3_perpend = new Line();
Line line4_perpend = new Line();
GetPerpendicular(centralPoint, line3, ref line3_perpend);
GetPerpendicular(centralPoint, line4, ref line4_perpend);

Vector3 roundnessPoint1 = GetCrossingPoint(line3, line3_perpend);
Vector3 roundnessPoint2 = GetCrossingPoint(line4, line4_perpend);*/


    /// <summary>
    /// ���������� �������� ��� �������������
    /// </summary>
    /// <param name="currentIndex"></param>
    /// <param name="v00">������ ����� ������ �������</param>
    /// <param name="v01">������ ����� ������� �������</param>
    /// <param name="v10">������ ������ ������ �������</param>
    /// <param name="v11">������ ������ ������� �������</param>
    private void GetGuard(ref int currentIndex, int v00, int v01, int v10, int v11 )
    {
        _triangles[currentIndex++] = v00;
        _triangles[currentIndex++] = v01;
        _triangles[currentIndex++] = v11;

        _triangles[currentIndex++] = v00;
        _triangles[currentIndex++] = v11;
        _triangles[currentIndex++] = v10;
    }

    /// <summary>
    /// ��������� ������, ���������������� ������ � ���������� ���� �����
    /// </summary>
    /// <param name="point">�����, ����� ������� �������� �������������</param>
    /// <param name="line"></param>
    /// <param name="linePerpendicular"></param>
    private void GetPerpendicular(Vector3 point, Line line, ref Line linePerpendicular)
    {
        linePerpendicular.k = -1f / line.k;
        linePerpendicular.b = point.x / line.k + point.z;
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
        float b1 = line1.b;
        float k2 = line2.k; 
        float b2 = line2.b;

        return new Vector3( (b2 - b1)/(k1 - k2), 0, (k1 * b2 - k2 * b1) /(k1 - k2) );
    }

    /// <summary>
    /// �����  ������, ������������ ������ point1, point2
    /// </summary>
    /// <param name="point1">start</param>
    /// <param name="point2">end</param>
    /// <param name="width">distance</param>
    /// <param name="k"></param>
    /// <param name="b"></param>
    private Vector3 GetLine(Vector3 point1, Vector3 point2, float width, ref Line line) 
    { 
        Vector3 road = new Vector3(point2.x - point1.x, point2.y - point1.y, point2.z - point1.z);
        Vector3 normal = new Vector3(road.z, 0, -road.x).normalized;

        Vector3 point3 = point1 + normal * width;
        Vector3 point4 = point2 + normal * width;

        //LineStandartForm(point3, point4, ref line);

        line = new Line(point3, point4);

        return normal;
    }

/*    /// <summary>
    /// �������� ��������� ������ � ����������� �����
    /// </summary>
    /// <param name="point1">point on line</param>
    /// <param name="point2">point on line</param>
    /// <param name="k">koefficient of line</param>
    /// <param name="b">koefficient of line</param>
    private void LineStandartForm(Vector3 point1, Vector3 point2, ref Line line)
    {
        line.k = (point1.z - point2.z) / (point1.x - point2.x);
        line.b = point2.z - point2.x * line.k;
    }*/
}
