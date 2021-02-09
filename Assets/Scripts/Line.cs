using UnityEngine;

public struct Line
{
    public Vector3 point1;
    public Vector3 point2;

    public float a;
    public float b;
    public float c;

    /// <summary>
    /// constructor: two-points form
    /// </summary>
    /// <param name="point1"></param>
    /// <param name="point2"></param>
    public Line(Vector3 point1, Vector3 point2)
    {
        a = b = c = 0f;

        this.point1 = point1;
        this.point2 = point2;

        GetGeneralForm(point1, point2);
    }

    /// <summary>
    /// переход к общему уравнению прямой
    /// </summary>
    /// <param name="point1">точка, через которую проходит прямая</param>
    /// <param name="point2">точка, через которую проходит прямая</param>
    public void GetGeneralForm(Vector3 point1, Vector3 point2)
    {
        a = point1.z - point2.z;
        b = point2.x - point1.x;
        c = point1.x * point2.z - point2.x * point1.z;
    }
}
