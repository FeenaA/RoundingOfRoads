using System;
using System.Collections.Generic;
using UnityEngine;

public class RoadGenerator
{
    /// <summary>
    /// ширина дорог
    /// </summary>
    private float _roadWidth;
    private float halfWidth;

    /// <summary>
    /// малый радиус скругления
    /// </summary>
    private readonly float radius = 1f;

    /// <summary>
    /// количество промежуточных точек на скруглении
    /// </summary>
    private readonly int _count = 5;

    private readonly List<Vector3> _verticesList = new List<Vector3>();
    private readonly List<int> _trianglesList = new List<int>();
    private readonly List<Vector3> _normalsList = new List<Vector3>();

    //    int count = (int)(Mathf.Abs(angle) / maxAngle) + 1;

    /// <summary>
    /// создать и заполнить mesh
    /// </summary>
    /*    public Mesh GetMesh(Vector3[] pointsInput, float roadWidth)
        {
            float sqrMinDistance = 2 * radius + _roadWidth;
            sqrMinDistance *= sqrMinDistance;

            Vector3[] points = RemoveSeriasOfPoints(pointsInput, sqrMinDistance);
            // проверка, является ли дорога кольцом
            bool isCycled = ArePointsClose(points[0], points[points.Length - 1], sqrMinDistance);

            _roadWidth = roadWidth;
            halfWidth = _roadWidth / 2f;

            _normalsList.Add(Vector3.up);
            _normalsList.Add(Vector3.up);

            int currentIndex = 0;

            //if (!isCycled)
            {
                // обработка первого угла
                var point1 = points[0];
                var pointCentre = points[1];
                var point2 = points[2];

                var (p1, p2, pC, rot) = ChangeSystemOld2New(point1, point2, pointCentre);

                var rot1 = Quaternion.Inverse(rot);

                // first section
                _verticesList.Add(rot1 * new Vector3(halfWidth, 0f, 0f) + point1);
                _verticesList.Add(rot1 * new Vector3(-halfWidth, 0f, 0f) + point1);

                var currentVertex = _verticesList.Count;

                GenerateVerticesTriangles(p1, p2, pC, ref currentIndex);
                ChangeSystemNew2Old(point1, rot, currentVertex);
            }

            int iterationCount = points.Length - 1;

            // middle corners
            for (int i = 2; i < iterationCount; i++)
            {
                var point1 = points[i - 1];
                var pointCentre = points[i];
                var point2 = points[i + 1];

                var currentVertex = _verticesList.Count;

                var (p1, p2, pC, rot) = ChangeSystemOld2New(point1, point2, pointCentre);

                GenerateVerticesTriangles(p1, p2, pC, ref currentIndex);
                ChangeSystemNew2Old(point1, rot, currentVertex);
            }

            // --- обработка "бублика"
            // если точки совпадают, сделать ещё одну итерацию



            // иначе - закрытие дороги
            // last section
            AddLastSection(points[iterationCount - 1], points[iterationCount], currentIndex);

            // ---

            Mesh mesh = new Mesh
            {
                vertices = _verticesList.ToArray(),
                triangles = _trianglesList.ToArray(),
                normals = _normalsList.ToArray(),
            };

            return mesh;
        }*/

    public Mesh GetMesh(Vector3[] pointsInput, float roadWidth)
    {
        float sqrMinDistance = 2 * radius + _roadWidth;
        sqrMinDistance *= sqrMinDistance;

        Vector3[] points = RemoveSeriasOfPoints(pointsInput, sqrMinDistance);
        // проверка, является ли дорога кольцом
        bool isCycled = ArePointsClose(points[0], points[points.Length - 1], sqrMinDistance);

        int iterationCount = points.Length - 1;
        int iterationStart = 2;

        _roadWidth = roadWidth;
        halfWidth = _roadWidth / 2f;

        int currentIndex = 0;

        // обработка первого угла
        if (isCycled)
        {
            var point1 = points[0];
            var pointCentre = points[1];
            var point2 = points[2];

            var (p1, p2, pC, rot) = ChangeSystemOld2New(point1, point2, pointCentre);

            var currentVertex = _verticesList.Count;

            GenerateVerticesTriangles(p1, p2, pC, ref currentIndex);
            ChangeSystemNew2Old(point1, rot, currentVertex);

            iterationStart = 1;
        }
        else
        {
            // correct

            var point1 = points[0];
            var pointCentre = points[1];
            var point2 = points[2];

            _normalsList.Add(Vector3.up);
            _normalsList.Add(Vector3.up);

            var (p1, p2, pC, rot) = ChangeSystemOld2New(point1, point2, pointCentre);

            var rot1 = Quaternion.Inverse(rot);

            // first section
            _verticesList.Add(rot1 * new Vector3(halfWidth, 0f, 0f) + point1);
            _verticesList.Add(rot1 * new Vector3(-halfWidth, 0f, 0f) + point1);

            var currentVertex = _verticesList.Count;

            GenerateVerticesTriangles(p1, p2, pC, ref currentIndex);
            ChangeSystemNew2Old(point1, rot, currentVertex);
        }

        // middle corners
        for (int i = iterationStart; i < iterationCount; i++)
        {
            currentIndex = GetGuard(currentIndex);

            var point1 = points[i - 1];
            var pointCentre = points[i];
            var point2 = points[i + 1];

            var currentVertex = _verticesList.Count;

            var (p1, p2, pC, rot) = ChangeSystemOld2New(point1, point2, pointCentre);

            GenerateVerticesTriangles(p1, p2, pC, ref currentIndex);
            ChangeSystemNew2Old(point1, rot, currentVertex);

            
        }
        currentIndex = GetGuard(currentIndex);

        if (!isCycled)
        {
            AddExtremeSection(points[iterationCount - 1], points[iterationCount], currentIndex );
        }
        else
        {
            var point1 = points[iterationCount-1];
            var pointCentre = points[iterationCount];
            var point2 = points[1];

            var currentVertex = _verticesList.Count;

            var (p1, p2, pC, rot) = ChangeSystemOld2New(point1, point2, pointCentre);

            GenerateVerticesTriangles(p1, p2, pC, ref currentIndex);
            ChangeSystemNew2Old(point1, rot, currentVertex);

            _trianglesList.Add(currentIndex);
            _trianglesList.Add(currentIndex + 1);
            _trianglesList.Add(1);

            _trianglesList.Add(currentIndex);
            _trianglesList.Add(1);
            _trianglesList.Add(0);
        }

        Mesh mesh = new Mesh
        {
            vertices = _verticesList.ToArray(),
            triangles = _trianglesList.ToArray(),
            normals = _normalsList.ToArray(),
        };

        return mesh;
    }

    /// <summary>
    /// исключить из рассмотрения дублирующиеся точки, идущие подряд
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    private Vector3[] RemoveSeriasOfPoints(Vector3[] points, float sqrMinDistance)
    {
        List<Vector3> res = new List<Vector3>();
        Vector3 currentPoint = points[0];

        for (int i = 1; i < points.Length; i++)
        {
            // если точки расположены близко друг к другу, заменить их средней точкой
            if (ArePointsClose(currentPoint, points[i], sqrMinDistance))
            {
                currentPoint = new Vector3(
                    (currentPoint.x + points[i].x) / 2f,
                    0f,
                    (currentPoint.z + points[i].z) / 2f
                    );
            }
            else
            {
                res.Add(currentPoint);
                currentPoint = points[i];
            }
        }
        res.Add(currentPoint);
        return res.ToArray();
    }

    /// <summary>
    /// Проверка на близость двух точек
    /// </summary>
    /// <param name="point1"></param>
    /// <param name="point2"></param>
    /// <param name="sqrDistance">Квадрат минимального расстояния между вершинами</param>
    /// <returns></returns>
    private bool ArePointsClose(Vector3 point1, Vector3 point2, float sqrDistance)
    {
        float deltaX = point1.x - point2.x;
        float deltaZ = point1.z - point2.z;

        return ((deltaX * deltaX + deltaZ * deltaZ) < sqrDistance);
    }

    /// <summary>
    /// добавить конечные треугольники
    /// </summary>
    /// <param name="lastButOnePoint">предпоследняя точка</param>
    /// <param name="lastPoint">последняя точка</param>
    private void AddExtremeSection(Vector3 lastButOnePoint, Vector3 lastPoint, int currentIndex)
    {
        Vector3 road = lastButOnePoint - lastPoint;
        Vector3 normal = new Vector3(road.z, 0, -road.x).normalized * halfWidth;

        _verticesList.Add(lastPoint - normal);
        _verticesList.Add(lastPoint + normal);

        _normalsList.Add(Vector3.up);
        _normalsList.Add(Vector3.up);

        GetGuard(currentIndex);
    }

    /// <summary>
    /// change initial system to temporary 
    /// </summary>
    private (Vector3 p1, Vector3 p2, Vector3 pC, Quaternion rotation) ChangeSystemOld2New(Vector3 point1, Vector3 point2, Vector3 pointCentre)
    {
        // change an origin point
        var pC = pointCentre - point1;
        var p2 = point2 - point1;
        var p1 = Vector3.zero;

        // get point direction relative to pivot
        Vector3 dir = pC - p1;
        var s = -1 * Mathf.Sign(pC.x);
        var angle = s * Vector3.Angle(dir, Vector3.forward);

        var rotation = Quaternion.AngleAxis(angle, Vector3.up);

        // calculate rotated points
        pC = rotation * pC;
        p2 = rotation * p2;

        return (p1, p2, pC, rotation);
    }

    /// <summary>
    /// move _vertices to old basis
    /// </summary>
    /// <param name="point1">pivot</param>
    private void ChangeSystemNew2Old(Vector3 point1, Quaternion rotation, int currentVertex)
    {
        var rot = Quaternion.Inverse(rotation);

        // move and rotate vertices
        for (int i = currentVertex; i < _verticesList.Count; i++)
        {
            _verticesList[i] = rot * _verticesList[i] + point1;
            _normalsList.Add(Vector3.up);
        }
    }

    /// <summary>
    /// генератор вершин и индексов
    /// </summary>
    /*private void GenerateVerticesTriangles(Vector3 point1, Vector3 point2, Vector3 pointCentre, ref int currentIndex)
    {
        int directionSign = DetectDirection(point1, point2, pointCentre);

        // получить центр окружностей
        Vector3 crossingPoint = GetCenterOfCircle(directionSign, point2, pointCentre);

        // получить точку скругления
        Vector3 road = point1 - pointCentre;
        Vector3 normal = new Vector3(road.z, 0, -road.x).normalized;
        Vector3 roundnessPoint = crossingPoint + directionSign * normal * radius;

        // определить сектор поворота
        Vector3 vectorStart = roundnessPoint - crossingPoint;
        Vector3 mediana = pointCentre - crossingPoint;
        float angleRoads = Vector3.Angle(vectorStart, mediana) * 2f;

        float deltaAngle = angleRoads / _count;
        float angleX = Vector3.Angle(Vector3.right, crossingPoint - roundnessPoint) - 90f;

        // построить скругленный участок
        int currentCount = (angleX == 0) ? 0 : _count;

        for (int i = 0; i <= currentCount; i++)
        {
            if (directionSign > 0)
            {
                _verticesList.Add(Rotate(radius, angleX + directionSign * i * deltaAngle, crossingPoint));
                _verticesList.Add(Rotate(radius + _roadWidth, angleX + directionSign * i * deltaAngle, crossingPoint));
            }
            else
            {
                _verticesList.Add(Rotate(radius + _roadWidth, angleX + directionSign * i * deltaAngle, crossingPoint));
                _verticesList.Add(Rotate(radius, angleX + directionSign * i * deltaAngle, crossingPoint));
            }

            currentIndex = GetGuard(currentIndex);
        }
    }*/

    private void GenerateVerticesTriangles(Vector3 point1, Vector3 point2, Vector3 pointCentre, ref int currentIndex)
    {
        int directionSign = DetectDirection(point1, point2, pointCentre);

        // получить центр окружностей
        Vector3 crossingPoint = GetCenterOfCircle(directionSign, point2, pointCentre);

        // получить точку скругления
        Vector3 road = point1 - pointCentre;
        Vector3 normal = new Vector3(road.z, 0, -road.x).normalized;
        Vector3 roundnessPoint = crossingPoint + directionSign * normal * radius;

        // определить сектор поворота
        Vector3 vectorStart = roundnessPoint - crossingPoint;
        Vector3 mediana = pointCentre - crossingPoint;
        float angleRoads = Vector3.Angle(vectorStart, mediana) * 2f;

        float deltaAngle = angleRoads / _count;
        float angleX = Vector3.Angle(Vector3.right, crossingPoint - roundnessPoint) - 90f;

        // построить скругленный участок
        int currentCount = (angleX == 0) ? 0 : _count;

        // нулевая итерация
        if (directionSign > 0)
        {
            _verticesList.Add(Rotate(radius, angleX, crossingPoint));
            _verticesList.Add(Rotate(radius + _roadWidth, angleX, crossingPoint));
        }
        else
        {
            _verticesList.Add(Rotate(radius + _roadWidth, angleX, crossingPoint));
            _verticesList.Add(Rotate(radius, angleX, crossingPoint));
        }

        for (int i = 1; i <= currentCount; i++)
        {
            if (directionSign > 0)
            {
                _verticesList.Add(Rotate(radius, angleX + directionSign * i * deltaAngle, crossingPoint));
                _verticesList.Add(Rotate(radius + _roadWidth, angleX + directionSign * i * deltaAngle, crossingPoint));
            }
            else
            {
                _verticesList.Add(Rotate(radius + _roadWidth, angleX + directionSign * i * deltaAngle, crossingPoint));
                _verticesList.Add(Rotate(radius, angleX + directionSign * i * deltaAngle, crossingPoint));
            }

            currentIndex = GetGuard(currentIndex);
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
    private int GetGuard(int currentIndex)
    {
        _trianglesList.Add(currentIndex);
        _trianglesList.Add(currentIndex + 1);
        _trianglesList.Add(currentIndex + 3);

        _trianglesList.Add(currentIndex);
        _trianglesList.Add(currentIndex + 3);
        _trianglesList.Add(currentIndex + 2);

        return currentIndex += 2;
    }

    /// <summary>
    /// найти центр окружностей
    /// </summary> 
    private Vector3 GetCenterOfCircle(int directionSign, Vector3 point2, Vector3 pointCentre)
    {
        float width = halfWidth + radius;

        GetParallelLine(pointCentre, point2, width, out var line2, directionSign);

        // get parametres of lines 
        float c1 = directionSign * width;

        float a2 = line2.a;
        float b2 = line2.b;
        float c2 = line2.c;

        // case of parallel lines
        if (b2 == 0f)
        {
            Vector3 road = pointCentre - Vector3.zero;
            Vector3 normal = new Vector3(road.z, 0, -road.x).normalized;
            return pointCentre + normal * width;
        }

        // not parallel lines
        return new Vector3(
                c1,
                0f,
                (a2 * c1 + c2) / -b2
            );
    }

    /// <summary>
    /// Определить, является ли угол тупым
    /// </summary>
    /// <returns></returns>
    private int DetectDirection(Vector3 point1, Vector3 point2, Vector3 pointCentre)
    {
        // вектора дорог исходят из общей точки
        Vector3 road1 = point1 - pointCentre;
        Vector3 road2 = point2 - pointCentre;

        // псевдоскалярное произведение - по формуле определителя
        float pseudoScalarProduct = road1.x * road2.z - road1.z * road2.x;

        return Math.Sign(pseudoScalarProduct);
    }

    /// <summary>
    /// найти  прямую, параллельную прямой point1, point2
    /// </summary>
    /// <param name="point1">start</param>
    /// <param name="point2">end</param>
    /// <param name="width">расстояние между дорогами</param>
    /// <param name="line"></param>
    /// <param name="isDirectionReversed"></param>
    /// <returns></returns>
    private Vector3 GetParallelLine(Vector3 point1, Vector3 point2, float width, out Line line, int sign)
    {
        Vector3 road = new Vector3(point2.x - point1.x, point2.y - point1.y, point2.z - point1.z);
        Vector3 normal = new Vector3(road.z, 0, -road.x).normalized;

        Vector3 point3 = point1 + sign * normal * width;
        Vector3 point4 = point2 + sign * normal * width;

        line = new Line(point3, point4);

        return normal;
    }
}
