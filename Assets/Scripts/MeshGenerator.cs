using UnityEngine;

public class MeshGenerator
{
    // базовые точки
    private Vector3 _pointCentre;
    private Vector3 _point1;
    private Vector3 _point2;

    /// <summary>
    /// ширина дорог
    /// </summary>
    private readonly float roadWidth = 2f;

    /// <summary>
    /// малый радиус скругления
    /// </summary>
    private readonly float radius = 1f;

    /// <summary>
    /// количество промежуточных точек на скруглении
    /// </summary>
    private int _count = 5;

    private Vector3[] _vertices;
    private int[] _triangles;

    private Vector3 roundnessPoint;
    private int currentIndex = 0;
    private Vector3 crossingPoint;
    private bool isCrossingPointValid;

    /// <summary>
    /// создать и заполнить mesh
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
    /// генератор вершин и индексов
    /// </summary>
    private void GenerateVerticesTriangles()
    {
        // получить центр окружностей
        GetCrossingPoint();

        // выделить память для вершин и индексов
        AllocateMemory();
        
        // получить точку скругления
        GetRoundnessPoint();

        // --- проверка направления нормалей - для случая вывернутого угла

        // заполнить массивы вершин и индексов
        FillVerticesTriangles();
    }

    /// <summary>
    /// заполнить массивы вершин и индексов
    /// </summary>
    private void FillVerticesTriangles()
    {
        int currentVertic = 0;

        // получить точки первого края дороги
        GetStraightVertices(_point1, false, ref currentVertic);
          
        // определить сектор поворота
        Vector3 vectorStart = roundnessPoint - crossingPoint;
        Vector3 mediana = _pointCentre - crossingPoint;
        float angleRoads = Vector3.Angle(vectorStart, mediana) * 2f;

        // определить направление
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


        // построить скругленный участок
        int currentCount = (angleX == 0) ? 0 : _count;
        for (int i = 0; i <= currentCount; i++)
        {
            _vertices[currentVertic++] = Rotate(radius, angleX + i * deltaAngle, crossingPoint);
            _vertices[currentVertic++] = Rotate(radius + roadWidth, angleX + i * deltaAngle, crossingPoint);

            GetGuard(currentVertic - 4, currentVertic - 3, currentVertic - 2, currentVertic - 1);
        }
        // получить точки второго края дороги
        GetStraightVertices(_point2, true, ref currentVertic);

        GetGuard(currentVertic - 4, currentVertic - 3, currentVertic - 2, currentVertic - 1);
    }

    /// <summary>
    /// выделить память для вершин и индексов
    /// </summary>
    private void AllocateMemory()
    {
        int verticesCount;
        int guardCount;

        if (isCrossingPointValid)
        {
            // прямые под углом
            verticesCount = (4 + _count) * 2;
            guardCount = 2 + 1 + _count;
        }
        else
        {
            // угол вырожден
            verticesCount = 3 * 2;
            guardCount = 2;
        }

        _vertices = new Vector3[verticesCount];
        _triangles = new int[guardCount * 2 * 3];
    }

    /// <summary>
    /// получить точки начала скругления
    /// </summary>
    private void GetRoundnessPoint()
    {
        Vector3 road = _point1 - _pointCentre;
        Vector3 normal = new Vector3(road.z, 0, -road.x).normalized;
        roundnessPoint = crossingPoint + normal * radius;
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
    /// найти точку пересечения прямых 1) y = a*x + c, 2) y = b*x + d
    /// </summary>
    /// <param name="a"></param>
    /// <param name="c"></param>
    /// <param name="b"></param>
    /// <param name="d"></param>
    /// <returns>точка пересечения</returns>
    private void GetCrossingPoint()
    {
        // вектора дорог исходят из общей точки
        Vector3 road1 = _point1 - _pointCentre; 
        Vector3 road2 = _point2 - _pointCentre;

        // псевдоскалярное произведение - по формуле определителя
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
    /// переназначить дороги
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
    /// найти  прямую, параллельную прямой point1, point2
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
