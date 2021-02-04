using UnityEngine;

public class MeshGenerator 
{
    // input data
    Vector3 pointCentre = new Vector3(0, 0, 0);

    //// прямой угол
    //Vector3 point1 = new Vector3(-8, 0, -8);
    //Vector3 point2 = new Vector3(8, 0, -8);

    //// острый угол
    //Vector3 point1 = new Vector3(-5, 0, -8);
    //Vector3 point2 = new Vector3(5, 0, -8);

    // тупой угол
    Vector3 point1 = new Vector3(-9, 0, -5);
    Vector3 point2 = new Vector3(9, 0, -5);

    // ширина дорог
    // --- предусмотреть случай разных размеров
    private readonly float roadWidth = 3f;

    // малый радиус скругления
    // --- радиус рассчитывать
    private readonly float radius = 3f;

    /// <summary>
    /// количество промежуточных точек на скруглении
    /// </summary>
    private readonly int count = 5;// --- количество треугольников рассчитывать

    private Vector3[] _vertices;
    private int[] _triangles;
     
    private Vector3 roundnessPoint;
    private Vector3 RoundnessPoint;  

    private int currentIndex = 0;

    private Vector3 crossingPoint;

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

        int guardCount = 2 + 1 + count;
        _triangles = new int[guardCount * 2 * 3];

        // получить центр окружностей
        float width = roadWidth / 2f + radius;
        GetLine(point1, pointCentre, width, out var line1);
        GetLine(pointCentre, point2, width, out var line2);
        crossingPoint = GetCrossingPoint(line1, line2);

        // получить точки скругления
        GetRoundedPoints();

        float roadWidthHalf = roadWidth / 2f;

        // --- проверка направления нормалей

        int currentVertic = 0;

        // получить точки первого края дороги
        GetStraightVertices(point1, false, ref currentVertic);

        float angleRoads = Vector3.Angle(pointCentre - point2, point1 - pointCentre);
        float deltaAngle = (180f - angleRoads) / count;

        float angleX = Vector3.Angle(Vector3.left, crossingPoint - roundnessPoint);

        // построить скругленный участок
        for (int i = 0; i <= count; i++)
        {
            _vertices[currentVertic++] = Rotate(radius, angleX - 180f + i * deltaAngle, crossingPoint);
            _vertices[currentVertic++] = Rotate(radius + roadWidth, angleX - 180f + i * deltaAngle, crossingPoint);

            GetGuard(currentVertic - 4, currentVertic - 3, currentVertic - 2, currentVertic - 1);
        }
        // получить точки второго края дороги
        GetStraightVertices(point2, true, ref currentVertic);

        GetGuard(currentVertic - 4, currentVertic - 3, currentVertic - 2, currentVertic - 1);

    }

    /// <summary>
    /// получить точки начала скругления
    /// </summary>
    private void GetRoundedPoints()
    {
        // вариант для прямого угла
                Vector3 road = point1 - pointCentre; 
                Vector3 normal = new Vector3(road.z, 0, -road.x).normalized;
                roundnessPoint = crossingPoint + normal * radius;
                RoundnessPoint = crossingPoint + normal * (radius + roadWidth); 


       /* // вариант для тупого угла
        GetLine(point1, pointCentre, roadWidth / 2, out var line1);
        GetLine(pointCentre, point2, radius + roadWidth/2, out var line2);
        roundnessPoint = GetCrossingPoint(line1, line2);


        Vector3 road = point1 - pointCentre;
        Vector3 normal = new Vector3(road.z, 0, -road.x).normalized;
        
        RoundnessPoint = roundnessPoint + normal*roadWidth;

        // определить новую crossing point
        Line lineNormal = new Line(RoundnessPoint, roundnessPoint);
        Line lineCentral = new Line(pointCentre, crossingPoint); // частный случай - деление на нуль
        crossingPoint = GetCrossingPoint(lineNormal, lineCentral);

        // определить для неё новый радиус
        int x = 5;*/
    }

    /// <summary>
    /// вершины на прямых участках дорог
    /// </summary>
    /// <param name="point">вершина на краю дороги</param>
    /// <param name="currentNum">текущее количество вершин</param>
    /// <param name="roundnessPoint">начало скругления на маленьком радиусе</param>
    /// <param name="RoundnessPoint">начало скругления на большом радиусе</param>
    private void GetStraightVertices(Vector3 point, bool isInverted, ref int currentVertic)
    {
        float halfRoadWidth = roadWidth / 2f;

        Vector3 road = point - pointCentre;
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
    /// поворот вектора
    /// </summary>
    /// <param name="radius">радиус вращения</param>
    /// <param name="angle">угол вращения</param>
    /// <param name="center">точка вращения</param>
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
    /// заполнение индексов для треугольников
    /// </summary>
    /// <param name="currentIndex"></param>
    /// <param name="v00">индекс левой нижней вершины</param>
    /// <param name="v01">индекс левой верхней вершины</param>
    /// <param name="v10">индекс правой нижней вершины</param>
    /// <param name="v11">индекс правой верхней вершины</param>
    private void GetGuard(int v00, int v01, int v10, int v11 )
    {
        _triangles[currentIndex++] = v00;
        _triangles[currentIndex++] = v01;
        _triangles[currentIndex++] = v11;

        _triangles[currentIndex++] = v00;
        _triangles[currentIndex++] = v11;
        _triangles[currentIndex++] = v10;
    }

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
