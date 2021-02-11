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

    private int currentIndex = 0;



    /// <summary>
    /// создать и заполнить mesh
    /// </summary>
    public Mesh GetMesh(Vector3[] points, float roadWidth)
    {
        _roadWidth = roadWidth;
        halfWidth = _roadWidth / 2f;

        _verticesList.Add(new Vector3(halfWidth, 0f, 0f));
        _verticesList.Add(new Vector3(-halfWidth, 0f, 0f));

        int iterationCount = points.Length - 2;

        // middle corners
        for (int i = 1; i < 3 /*2*/ /*iterationCount*/; i++)
        {
            var point1 = points[i - 1];
            var pointCentre = points[i];
            var point2 = points[i + 1];

            var (p1, p2, pC, rot) = ChangeSystemOld2New(point1, point2, pointCentre);
            // mesh: первый прямоугольник и гармошка
            GenerateVerticesTriangles(p1, p2, pC);

            // --- поворачивать в старую систему координат только новый угол
            ChangeSystemNew2Old(points[i - 1], rot);

            // --- назначить новые две точки в конце текущей гармошки в качестве стартовых для нового угла


        }

        // построить последние две точки
        // построить последний прямоугольник

        //GetStraightVertices(points[points.Length - 1], -1);

        Mesh mesh = new Mesh
        {
            vertices = _verticesList.ToArray(),
            triangles = _trianglesList.ToArray(),
            normals = _normalsList.ToArray(),
        };

        return mesh;
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

        // rotation
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
    private void ChangeSystemNew2Old(Vector3 point1, Quaternion rotation)
    {
        // rotation
        var rot = Quaternion.Inverse(rotation);

        // move and rotate vertices
        for (int i = 0; i < _verticesList.Count; i++)
        {
            _verticesList[i] = rot * _verticesList[i] + point1;
            _normalsList.Add(Vector3.up);
        }
    }

    /// <summary>
    /// генератор вершин и индексов
    /// заполнить массивы вершин и индексов
    /// </summary>
    private void GenerateVerticesTriangles(Vector3 point1, Vector3 point2, Vector3 pointCentre)
    {
        int directionSign = DetectDirection(point1, point2, pointCentre);

        // получить центр окружностей
        Vector3 crossingPoint = GetCenterOfCircle(directionSign, point1, point2, pointCentre);

        // получить точку скругления
        Vector3 road = point1 - pointCentre;
        //Vector3 road = _pointCentre - _point2;
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

            GetGuard();
        }
    }

    /// <summary>
    /// вершины на прямых участках дорог
    /// </summary>
    /// <param name="point">конец первой или второй дороги</param>
    /// <param name="isInverted">прямой или обратный порядок</param>
    /// <param name="currentVertic">текущее количество вершин</param>
    private void GetStraightVertices(Vector3 point, Vector3 pointCentre, int isInverted)
    {
        Vector3 road = point - pointCentre;
        Vector3 normal = new Vector3(road.z, 0, -road.x).normalized * halfWidth;

        Vector3 vertic1 = point - isInverted * normal;
        Vector3 vertic2 = point + isInverted * normal;

        _verticesList.Add(vertic1);
        _verticesList.Add(vertic2);

        _normalsList.Add(Vector3.up);
        _normalsList.Add(Vector3.up);
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
    private void GetGuard()
    {
        _trianglesList.Add(currentIndex);
        _trianglesList.Add(currentIndex + 1);
        _trianglesList.Add(currentIndex + 3);

        _trianglesList.Add(currentIndex);
        _trianglesList.Add(currentIndex + 3);
        _trianglesList.Add(currentIndex + 2);

        currentIndex += 2;
    }

    /// <summary>
    /// найти центр окружностей
    /// </summary> 
    private Vector3 GetCenterOfCircle(int directionSign, Vector3 point1, Vector3 point2, Vector3 pointCentre)
    {
        float width = halfWidth + radius;
        GetParallelLine(point1, pointCentre, width, out var line1, directionSign);
        GetParallelLine(pointCentre, point2, width, out var line2, directionSign);

        // swap roads
        //if (directionSign < 0)
        //{
        //    SwapRoads(ref line1, ref line2);
        //}

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
            Vector3 road = pointCentre - point1;
            Vector3 normal = new Vector3(road.z, 0, -road.x).normalized;
            return pointCentre + normal * width;
        }

        // not parallel lines
        return new Vector3(
            (c2 * b1 - c1 * b2) / denominator,
            0,
            (a2 * c1 - a1 * c2) / denominator
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
    /// переназначить дороги
    /// </summary>
    private void SwapRoads(ref Line line1, ref Line line2)
    {
        Line tempLine = line1;
        line1 = line2;
        line2 = tempLine;

        //Vector3 tempPoint = _point1;
        //_point1 = _point2;
        //_point2 = tempPoint;

        //// remove the first 
        //_verticesList.RemoveAt(0);
        //_verticesList.RemoveAt(0);

        //GetStraightVertices(_point1, -1);
        //_verticesList.Add(new Vector3(halfWidth, 0f, 0f));
        //_verticesList.Add(new Vector3(-halfWidth, 0f, 0f));
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
