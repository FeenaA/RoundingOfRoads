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
    private readonly int _count = 5;

    private float angleAbove = 0f;

    //private float epsilon = 1E-5f;

    private Vector3[] _vertices;
    private int[] _triangles;

    private Vector3 roundnessPoint;
    private int currentIndex = 0;
    private Vector3 crossingPoint;
    private bool isCrossingPointValid;

    private float angle;

    /// <summary>
    /// создать и заполнить mesh
    /// </summary>
    public Mesh GetMesh(Vector3 pointCentre, Vector3 point1, Vector3 point2)
    {
        _pointCentre = pointCentre;
        _point1 = point1;
        _point2 = point2;

        ChangeSystemOld2New();
        GenerateVerticesTriangles();
        ChangeSystemNew2Old(point1);

        Mesh mesh = new Mesh 
        {
            vertices = _vertices,
            triangles = _triangles
        };

        mesh.RecalculateNormals();
        return mesh;
    }

    /// <summary>
    /// change initial system to temporary 
    /// </summary>
    private void ChangeSystemOld2New()
    {
        // change an origin point
        _pointCentre -= _point1;
        _point2 -= _point1;
        _point1 = Vector3.zero;

        Vector3 dir = _pointCentre - _point1; // get point direction relative to pivot
        var s = -1 * Mathf.Sign(_pointCentre.x);
        angle = s * Vector3.Angle(dir, Vector3.forward);
        
        var rot = Quaternion.AngleAxis(angle, Vector3.up); // rotate it
        _pointCentre = rot * _pointCentre; // calculate rotated point
        _point2 = rot * _point2; // calculate rotated point

        //ChangePointToTemp(ref _pointCentre);
        //ChangePointToTemp(ref _point2);
    } 

    /// <summary>
    /// move points to temporary basis
    /// </summary>
    /// <param name="point"></param>
    private void ChangePointToTemp(ref Vector3 point)
    {
        float angleUnder = Vector3.Angle(point, Vector3.right);
        float cos = Mathf.Cos(angleUnder * Mathf.Deg2Rad);
        float radiusRotation;
        if ((cos < Vector3.kEpsilon) && (cos > -Vector3.kEpsilon))
        {
            radiusRotation = Mathf.Abs(point.z);
        }
        else
        {
            radiusRotation = point.x / cos;
        }
        float angleRotation;

        // случай _pointCentre
        if (angleAbove == 0)
        {
            angleRotation = 90f;
            angleAbove = angleRotation - angleUnder;
        }
        // случай _point2
        else
        {
            angleRotation = angleAbove + angleUnder;
        }

        point.x = radiusRotation * Mathf.Cos(angleRotation * Mathf.Deg2Rad);
        point.z = radiusRotation * Mathf.Sin(angleRotation * Mathf.Deg2Rad);
    }

    private void ChangeSystemNew2Old(Vector3 point1)
    {
        Vector3 dir = _pointCentre - _point1; // get point direction relative to pivot
        //var s = -1 * Mathf.Sign(_pointCentre.x);
        //var angle = s * Vector3.Angle(dir, Vector3.forward);
        
        var rot = Quaternion.AngleAxis( - angle, Vector3.up); // rotate it
        //_pointCentre = rot * _pointCentre; // calculate rotated point
        //_point2 = rot * _point2; // calculate rotated point

        // vertices
        for (int i = 0; i < _vertices.Length; i++)
        {
            // rotate
            //ChangePointToOld(ref _vertices[i]);
            _vertices[i] = rot * _vertices[i]; // calculate rotated point 

            // move
            _vertices[i] += point1;
        }
    }
      
    private void ChangePointToOld(ref Vector3 point)
    {
        float angleUnder = Vector3.Angle(point, Vector3.right);
        float cos = Mathf.Cos(angleUnder * Mathf.Deg2Rad);
        float radiusRotation;
        if ((cos < Vector3.kEpsilon) && (cos > -Vector3.kEpsilon))
        {
            radiusRotation = Mathf.Abs(point.z);
        }
        else
        {
            radiusRotation = point.x / cos;
        }
        float angleRotation =  angleUnder - angleAbove;

        point.x = radiusRotation * Mathf.Cos(angleRotation * Mathf.Deg2Rad);
        point.z = radiusRotation * Mathf.Sin(angleRotation * Mathf.Deg2Rad);
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

        float deltaAngle = angleRoads / _count;
        float angleX = Vector3.Angle(Vector3.right, crossingPoint - roundnessPoint) - 90f;

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
        bool isDirectionReversed = DetectDirection();

        float width = roadWidth / 2f + radius;
        GetLine(_point1, _pointCentre, width, out var line1, isDirectionReversed);
        GetLine(_pointCentre, _point2, width, out var line2, isDirectionReversed);

        // swap roads
        if (isDirectionReversed)
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
        if (denominator == 0)
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



        //Quaternion.Ro



        /*        float k1 = line1.k;
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
                    (k1 * b2 - k2 * b1) / (k1 - k2));*/

        isCrossingPointValid = true;
    }

    /// <summary>
    /// Определить, является ли направление инвертированным
    /// </summary>
    /// <returns></returns>
    private bool DetectDirection()
    {
        // вектора дорог исходят из общей точки
        Vector3 road1 = _point1 - _pointCentre;
        Vector3 road2 = _point2 - _pointCentre;

        // псевдоскалярное произведение - по формуле определителя
        float pseudoScalarProduct = road1.x * road2.z - road1.z * road2.x;

        return pseudoScalarProduct < 0;
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
        if (isDirectionReversed)
        {
            sign = -1;
        }

        Vector3 point3 = point1 + sign * normal * width;
        Vector3 point4 = point2 + sign * normal * width;

        line = new Line(point3, point4);

        return normal;
    }
}
