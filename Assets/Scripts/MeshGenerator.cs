using UnityEngine;

public class MeshGenerator 
{
    // input data
    Vector3 pointCentre = new Vector3(0, 0, 0);
    Vector3 point1 = new Vector3(-5, 0, -5);
    Vector3 point2 = new Vector3(5, 0, -5);

    // ширина дорог
    // --- предусмотреть случай разных размеров
    private readonly float roadWidth = 5f;

    // малый радиус скругления
    // --- радиус рассчитывать
    private readonly float radius = 3f;

    /// <summary>
    /// количество промежуточных точек на скруглении
    /// </summary>
    private readonly int count = 10;// --- количество треугольников рассчитывать

    private Vector3[] _vertices;
    private int[] _triangles;
     
    private Vector3 roundnessPoint1;
    private Vector3 roundnessPoint2;
    private Vector3 RoundnessPoint1;
    private Vector3 RoundnessPoint2;

    /// <summary>
    /// создать и заполнить mesh
    /// </summary>
    public Mesh GetMesh()
    {
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
    /// генератор вершин и индексов
    /// </summary>
    private void GenerateVerticesTriangles()
    {
        _vertices = new Vector3[ (4 + count) * 2];

        int currentNum = 0;
        GetStraightSection(point1, ref currentNum, ref roundnessPoint1, ref RoundnessPoint1);
        GetRoundedSection(ref currentNum);
        GetStraightSection(point2, ref currentNum, ref roundnessPoint2, ref RoundnessPoint2);

        GenerateTriangles();
    }

    /// <summary>
    /// заполнение массива индексов для треугольников
    /// </summary>
    /// <returns>_triangles - массив индексов</returns>
    private int[] GenerateTriangles()
    { 
        int guardCount = 2 + 1 + count;
        _triangles = new int[guardCount * 2 * 3 ];

        int currentIndex = 0;
        int v = 0;
        for (int i = 0; i < guardCount - 1; i++)
        {
            GetGuard(ref currentIndex, v, v + 1, v + 2, v + 3);
            v += 2;
        }
        GetGuard(ref currentIndex, v, v, v, v);

        return _triangles;
    }

    /// <summary>
    /// вершины на прямых участках дорог
    /// </summary>
    /// <param name="point">вершина на краю дороги</param>
    /// <param name="currentNum">текущее количество вершин</param>
    /// <param name="roundnessPoint">начало скругления на маленьком радиусе</param>
    /// <param name="RoundnessPoint">начало скругления на большом радиусе</param>
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
        float roadWidthHalf = roadWidth / 2f;

        // --- проверка направления нормалей

        float width = roadWidthHalf + radius;
        GetLine(point1, pointCentre, width, out var line1);
        GetLine(pointCentre, point2, width, out var line2);

        Vector3 crossingPoint = GetCrossingPoint(line1, line2);
         
        float angleRoads = Vector3.Angle(pointCentre - point2, point1 - pointCentre);
        float deltaAngle = (180f - angleRoads) / (count + 1);

        float angleX = Vector3.Angle(Vector3.left, crossingPoint - roundnessPoint1);

        var x = Rotate(radius, angleX - 180f, crossingPoint);

        for (int i = 1; i <= count; i++)
        {
            _vertices[currentNum++] = Rotate(radius, angleX - 180f + i * deltaAngle, crossingPoint);
            _vertices[currentNum++] = Rotate(radius + roadWidth, angleX - 180f + i * deltaAngle, crossingPoint);
        }
    }

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
    /// заполнение индексов для треугольников
    /// </summary>
    /// <param name="currentIndex"></param>
    /// <param name="v00">индекс левой нижней вершины</param>
    /// <param name="v01">индекс левой верхней вершины</param>
    /// <param name="v10">индекс правой нижней вершины</param>
    /// <param name="v11">индекс правой верхней вершины</param>
    private void GetGuard(ref int currentIndex, int v00, int v01, int v10, int v11 )
    {
        _triangles[currentIndex++] = v00;
        _triangles[currentIndex++] = v01;
        _triangles[currentIndex++] = v11;

        _triangles[currentIndex++] = v00;
        _triangles[currentIndex++] = v11;
        _triangles[currentIndex++] = v10;
    }

/*    /// <summary>
    /// построить прямую, перпендикулярную данной и проходящую черз точку
    /// </summary>
    /// <param name="point">точка, через которую проходит перпендикуляр</param>
    /// <param name="line"></param>
    /// <param name="linePerpendicular"></param>
    private void GetPerpendicular(Vector3 point, Line line, ref Line linePerpendicular)
    {
        linePerpendicular.k = -1f / line.k;
        linePerpendicular.b = point.x / line.k + point.z;
    }*/

    /// <summary>
    /// найти точку пересечения прямых 1) y = a*x + c, 2) y = b*x + d
    /// </summary>
    /// <param name="a"></param>
    /// <param name="c"></param>
    /// <param name="b"></param>
    /// <param name="d"></param>
    /// <returns>точка пересечения</returns>
    private Vector3 GetCrossingPoint(Line line1, Line line2) 
    {
        float k1 = line1.k;
        float b1 = line1.b;
        float k2 = line2.k; 
        float b2 = line2.b;

        return new Vector3( (b2 - b1)/(k1 - k2), 0, (k1 * b2 - k2 * b1) /(k1 - k2) );
    }

    /// <summary>
    /// найти  прямую, параллельную прямой point1, point2
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
