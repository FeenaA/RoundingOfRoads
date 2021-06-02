using SmartTwin.Utils.Models.Geometry;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class RoadMeshGenerator2
{
    /// <summary>
    /// создать и заполнить mesh
    /// </summary>
    /// <param name="roadVertices">Последовательность точек, представляющих дорогу</param>
    /// <param name="width">Ширина дороги</param>
    /// <param name="radius">Радиус скругления</param>
    /// <param name="maxAngle">Максимальный допустимый угол для одного сектора поворота</param>
    /// <returns></returns>
    public static Mesh GenerateRoadMesh(List<Vector3> roadVertices, float width, float radius, float maxAngle = 10f)
    {
        if (roadVertices.Count <= 1)
            throw new ArgumentException("Number of points is less than 2");

        MeshInfo res = new MeshInfo().Init();

        // preparation----------------------------------------------------------------
        float halfWidth = width / 2f;

        float sqrMinDistance = 2f * radius + width;
        sqrMinDistance *= sqrMinDistance;

        Vector3[] points = RemoveSeriasOfPoints(roadVertices, sqrMinDistance);

        if (points.Length < 3)
        {
            if (points.Length == 2)
            {
                TwoPointsRoad(points[0], points[1], halfWidth, res);

                Mesh mesh1 = new Mesh
                {
                    vertices = res.vertices.ToArray(),
                    triangles = res.indexes.ToArray(),
                    normals = res.normals.ToArray(),
                };
                return mesh1;
                //return res;
            }
            else
            {
                return null;
            }
        }

        bool isCycled = PointsClose(points[0], points[points.Length - 1], sqrMinDistance);
        // first section-------------------------------------------------------------------
        {
            var point1 = points[0];
            var point2 = points[2];

            var (p1, p2, pC, rot) = ChangeSystemOld2New(point1, point2, points[1]);

            GetGuard(res);

            // граф не является циклом
            if (!isCycled)
            {
                var rot1 = Quaternion.Inverse(rot);

                res.vertices.Add(rot1 * new Vector3(halfWidth, 0f, 0f) + point1);
                res.vertices.Add(rot1 * new Vector3(-halfWidth, 0f, 0f) + point1);
            }

            var currentVertex = res.vertices.Count;
            GenerateVerticesTriangles(p1, p2, pC, radius, maxAngle, width, res);
            /*var (pp1, pp2) = */ChangeSystemNew2Old(point1, rot, currentVertex, res);
             
            // разделить прямые участки на квадраты
            //AddVerticesForTexture(res.vertices[0], res.vertices[1], pp1, pp1, res);
        }

        // middle corners
        //Vector3 start1, start2, fin1, fin2;
        int iterationCount = points.Length - 1;
        int iterationStart = (isCycled == true) ? 1 : 2;
        for (int i = iterationStart; i < iterationCount; i++)
        {
            GetGuard(res);
            CompleteCorner(points[i - 1], points[i + 1], points[i], radius, maxAngle, width, res);
        }

        // разделить прямые участки на квадраты
        //AddVerticesForTexture(start1, start2, fin1, fin2, res);

        // last section
        if (!isCycled)
        {
            AddExtremeSection(points[iterationCount - 1], points[iterationCount], halfWidth, res);
        }
        else
        {
            CompleteCorner(points[iterationCount - 1], points[1], points[iterationCount], radius, maxAngle, width, res);

            int currentIndex = (res.indexes.Count == 0) ? 0 : res.indexes[res.indexes.Count - 1];

            res.indexes.Add(currentIndex);
            res.indexes.Add(currentIndex + 1);
            res.indexes.Add(1);

            res.indexes.Add(currentIndex);
            res.indexes.Add(1);
            res.indexes.Add(0);
        }

        FillNormals(res);

        Mesh mesh = new Mesh
        {
            vertices = res.vertices.ToArray(),
            triangles = res.indexes.ToArray(),
            normals = res.normals.ToArray(),
        };

        return mesh;// dataModel.meshInfo;
    }


    /// <summary>
    /// разделить прямые участки на квадраты
    /// </summary>
    /// <param name=""></param>
    /// <param name=""></param>
    /// <param name=""></param>
    /// <param name=""></param>
    private static void AddVerticesForTexture(Vector3 start1, Vector3 start2, Vector3 fin1, Vector3 fin2, MeshInfo mesh)
    {
 /*       // построение квадратов
        // количество текстурных единиц на прямой 
        float textureCountFloat = roundnessPoint.z / width;
        int textureCount = (int)textureCountFloat;
        // размер ячейки
        float sizeCell = roundnessPoint.z / textureCount;
        float initialSizeCell = sizeCell;


        for (int i = 0; i < textureCount - 1; i++)
        {
            // добавить вершины
            if (directionSign > 0)
            {
                mesh.vertices.Add(new Vector3(roundnessPoint.x, 0f, sizeCell));
                mesh.vertices.Add(new Vector3(-roundnessPoint.x, 0f, sizeCell));
            }
            else
            {
                mesh.vertices.Add(new Vector3(-roundnessPoint.x, 0f, sizeCell));
                mesh.vertices.Add(new Vector3(roundnessPoint.x, 0f, sizeCell));
            }

            // todo - добавить текстурные координаты
            sizeCell += initialSizeCell;

            GetGuard(mesh);
        }*/
    }



    /// <summary>
    /// заполнить список нормалей
    /// </summary>
    /// <param name="mesh"></param>
    private static void FillNormals(MeshInfo mesh)
    {
        int N = mesh.vertices.Count;
        for (int i = 0; i < N; i++)
        {
            mesh.normals.Add(Vector3.up);
        }
    }


    /// <summary>
    /// обработка случая дороги, содержащей две опорные точки
    /// </summary>
    /// <param name="point1"></param>
    /// <param name="point2"></param>
    /// <param name="currentIndex"></param>
    /// <param name="halfWidth"></param>
    /// <param name="mesh"></param>
    private static void TwoPointsRoad(Vector3 point1, Vector3 point2, float halfWidth, MeshInfo mesh)
    {
        Vector3 road = point2 - point1;
        Vector3 normal = new Vector3(road.z, 0, -road.x).normalized * halfWidth;

        mesh.vertices.Add(point1 - normal);
        mesh.vertices.Add(point1 + normal);

        mesh.vertices.Add(point2 - normal);
        mesh.vertices.Add(point2 + normal);

        // заполнить нормали
        FillNormals(mesh);
        // заполнить индексы
        GetGuard(mesh);
    }

    /// <summary>
    /// complete mesh at rounding corners
    /// </summary>
    /// <param name="point1"></param>
    /// <param name="point2"></param>
    /// <param name="pointCentre"></param>
    /// <param name="currentIndex"></param>
    private static void CompleteCorner(Vector3 point1, Vector3 point2, Vector3 pointCentre,
        float radius, float maxAngle, float width, MeshInfo mesh)
    {
        var (p1, p2, pC, rot) = ChangeSystemOld2New(point1, point2, pointCentre);

        var currentVertex = mesh.vertices.Count;
        GenerateVerticesTriangles(p1, p2, pC, radius, maxAngle, width, mesh);
        ChangeSystemNew2Old(point1, rot, currentVertex, mesh);
    }

    /// <summary>
    /// исключить из рассмотрения дублирующиеся точки, идущие подряд
    /// </summary>
    /// <param name="roadVertices"></param>
    /// <param name="sqrMinDistance"></param>
    /// <returns></returns>
    private static Vector3[] RemoveSeriasOfPoints(List<Vector3> roadVertices, float sqrMinDistance)
    {
        List<Vector3> res = new List<Vector3>();
        Vector3 currentPoint = roadVertices[0];

        for (int i = 1; i < roadVertices.Count; i++)
        {
            // если точки расположены близко друг к другу, заменить их средней точкой
            if (PointsClose(currentPoint, roadVertices[i], sqrMinDistance))
            {
                currentPoint = new Vector3(
                    (currentPoint.x + roadVertices[i].x) / 2f,
                    0f,
                    (currentPoint.z + roadVertices[i].z) / 2f
                    );
            }
            else
            {
                res.Add(currentPoint);
                currentPoint = roadVertices[i];
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
    private static bool PointsClose(Vector3 point1, Vector3 point2, float sqrDistance)
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
    private static void AddExtremeSection(Vector3 lastButOnePoint, Vector3 lastPoint, float halfWidth, MeshInfo mesh)
    {
        Vector3 road = lastButOnePoint - lastPoint;
        Vector3 normal = new Vector3(road.z, 0, -road.x).normalized * halfWidth;

        mesh.vertices.Add(lastPoint - normal);
        mesh.vertices.Add(lastPoint + normal);

        GetGuard(mesh);
    }

    /// <summary>
    /// change initial system to temporary 
    /// </summary>
    private static (Vector3 p1, Vector3 p2, Vector3 pC, Quaternion rotation) ChangeSystemOld2New(Vector3 point1, Vector3 point2, Vector3 pointCentre)
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
    private static (Vector3 p1, Vector3 p2) ChangeSystemNew2Old(Vector3 point1, Quaternion rotation, int currentVertex, MeshInfo mesh)
    {
        var rot = Quaternion.Inverse(rotation);
        int N = mesh.vertices.Count;

        mesh.vertices[currentVertex] = rot * mesh.vertices[currentVertex] + point1;
        mesh.vertices[currentVertex + 1] = rot * mesh.vertices[currentVertex + 1] + point1;
        var p1 = mesh.vertices[currentVertex];
        var p2 = mesh.vertices[currentVertex + 1];

        // move and rotate vertices
        for (int i = currentVertex + 2; i < N; i++)
        {
            mesh.vertices[i] = rot * mesh.vertices[i] + point1;
        }

        return (p1, p2);
    }

    /// <summary>
    /// генератор вершин и индексов
    /// </summary>
    private static void GenerateVerticesTriangles(Vector3 point1, Vector3 point2, Vector3 pointCentre,
        float radius, float maxAngle, float width, MeshInfo mesh)
    {
        int directionSign = DetectDirection(point1, point2, pointCentre);

        // получить центр окружностей
        Vector3 crossingPoint = GetCenterOfCircle(directionSign, point2, pointCentre, width / 2 + radius);

        // текущее количество индексов
        //int indexesCount = mesh.indexes.Count;

        // получить точку скругления
        Vector3 road = point1 - pointCentre;
        Vector3 normal = new Vector3(road.z, 0, -road.x).normalized;
        Vector3 roundnessPoint = crossingPoint + directionSign * normal * radius;

        // определить сектор поворота
        Vector3 vectorStart = roundnessPoint - crossingPoint;
        Vector3 mediana = pointCentre - crossingPoint;
        float angleRoads = Vector3.Angle(vectorStart, mediana) * 2f;

        // количество точек разбиения
        int count = (int)(Mathf.Abs(angleRoads) / maxAngle) + 1;

        float deltaAngle = angleRoads / count;
        float angleX = Vector3.Angle(Vector3.right, crossingPoint - roundnessPoint) - 90f;

        // нулевая итерация
        if (directionSign > 0)
        {
            mesh.vertices.Add(Rotate(radius, angleX, crossingPoint));
            mesh.vertices.Add(Rotate(radius + width, angleX, crossingPoint));
        }
        else
        {
            mesh.vertices.Add(Rotate(radius + width, angleX, crossingPoint));
            mesh.vertices.Add(Rotate(radius, angleX, crossingPoint));
        }

        for (int i = 1; i <= count; i++)
        {
            if (directionSign > 0)
            {
                mesh.vertices.Add(Rotate(radius, angleX + directionSign * i * deltaAngle, crossingPoint));
                mesh.vertices.Add(Rotate(radius + width, angleX + directionSign * i * deltaAngle, crossingPoint));
            }
            else
            {
                mesh.vertices.Add(Rotate(radius + width, angleX + directionSign * i * deltaAngle, crossingPoint));
                mesh.vertices.Add(Rotate(radius, angleX + directionSign * i * deltaAngle, crossingPoint));
            }

            GetGuard(mesh);
        }


        // построение квадратов
        // количество текстурных единиц на прямой 
        /*float oldZ = 0f;
        float textureCountFloat = (roundnessPoint.z - oldZ) / width;
        int textureCount = (int)textureCountFloat;
        // размер ячейки
        float sizeCell = roundnessPoint.z / textureCount;
        float initialSizeCell = sizeCell;

        // нулевая итерация
        if (directionSign > 0)
        {
            mesh.vertices.Add(new Vector3(roundnessPoint.x, 0f, sizeCell));
            mesh.vertices.Add(new Vector3(-roundnessPoint.x, 0f, sizeCell));
        }
        else
        {
            mesh.vertices.Add(new Vector3(-roundnessPoint.x, 0f, sizeCell));
            mesh.vertices.Add(new Vector3(roundnessPoint.x, 0f, sizeCell));
        }
        // todo - добавить текстурные координаты
        sizeCell += initialSizeCell;

        int currentIndex = (indexesCount == 0) ? 0 : mesh.indexes[indexesCount - 1];

        mesh.indexes.Add(currentIndex);
        mesh.indexes.Add(currentIndex + 1);
        mesh.indexes.Add(currentIndex + 3);

        mesh.indexes.Add(currentIndex);
        mesh.indexes.Add(currentIndex + 3);
        mesh.indexes.Add(currentIndex + 2);


        // цикл
        for (int i = 1; i < textureCount - 1; i++)
        {
            // добавить вершины
            if (directionSign > 0)
            {
                mesh.vertices.Add(new Vector3(roundnessPoint.x, 0f, sizeCell));
                mesh.vertices.Add(new Vector3(-roundnessPoint.x, 0f, sizeCell));
            }
            else
            {
                mesh.vertices.Add(new Vector3(-roundnessPoint.x, 0f, sizeCell));
                mesh.vertices.Add(new Vector3(roundnessPoint.x, 0f, sizeCell));
            }

            // todo - добавить текстурные координаты
            sizeCell += initialSizeCell;

            currentIndex = (mesh.indexes.Count == 0) ? 0 : mesh.indexes[mesh.indexes.Count - 1];

            mesh.indexes.Add(currentIndex);
            mesh.indexes.Add(currentIndex + 1);
            mesh.indexes.Add(currentIndex + 3);

            mesh.indexes.Add(currentIndex);
            mesh.indexes.Add(currentIndex + 3);
            mesh.indexes.Add(currentIndex + 2);
        }*/
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
    private static void GetGuard(MeshInfo mesh)
    {
        int currentIndex = (mesh.indexes.Count == 0) ? 0 : mesh.indexes[mesh.indexes.Count - 1];

        mesh.indexes.Add(currentIndex);
        mesh.indexes.Add(currentIndex + 1);
        mesh.indexes.Add(currentIndex + 3);

        mesh.indexes.Add(currentIndex);
        mesh.indexes.Add(currentIndex + 3);
        mesh.indexes.Add(currentIndex + 2);
    }

    /// <summary>
    /// найти центр окружностей
    /// </summary> 
    private static Vector3 GetCenterOfCircle(int directionSign, Vector3 point2, Vector3 pointCentre, float distance)
    {
        GetParallelLine(pointCentre, point2, distance, out var line2, directionSign);

        // get parametres of lines 
        float c1 = directionSign * distance;

        float a2 = line2.a;
        float b2 = line2.b;
        float c2 = line2.c;

        // case of parallel lines
        if ((b2 < Mathf.Epsilon) && (b2 > -Mathf.Epsilon))
        {
            Vector3 normal = new Vector3(pointCentre.z, 0, -pointCentre.x).normalized;
            return pointCentre + normal * distance;
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
    private static int DetectDirection(Vector3 point1, Vector3 point2, Vector3 pointCentre)
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
    private static Vector3 GetParallelLine(Vector3 point1, Vector3 point2, float width, out Line line, int sign)
    {
        Vector3 road = new Vector3(point2.x - point1.x, point2.y - point1.y, point2.z - point1.z);
        Vector3 normal = new Vector3(road.z, 0, -road.x).normalized;

        Vector3 vector = sign * normal * width;
        Vector3 point3 = point1 + vector;
        Vector3 point4 = point2 + vector;

        line = new Line(point3, point4);

        return normal;
    }
}
