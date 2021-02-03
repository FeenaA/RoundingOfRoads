using UnityEngine;

public struct Line
{
    private float _k;
    private float _b;

    public float k
    {
        get { return _k; }
        set { _k = value; }
    }

    public float b
    {
        get { return _b; }
        set { _b = value; }
    }

    public Vector3 point1;
    public Vector3 point2;

    private Vector3 _point1;
    private Vector3 _point2;

    /// <summary>
    /// constructor: standart form
    /// </summary>
    /// <param name="k"></param>
    /// <param name="b"></param>
    public Line(float k, float b)
    {
        point1 = Vector3.zero;
        point2 = Vector3.zero;

        _point1 = Vector3.zero;
        _point2 = Vector3.zero;

        _k = k;
        _b = b;

        // GetTwoPointsForm();
    }

    //public Line() : this(0f, 0f)
    //{
    //}

    /// <summary>
    /// constructor: two-points form
    /// </summary>
    /// <param name="point1"></param>
    /// <param name="point2"></param>
    public Line(Vector3 point1, Vector3 point2)
    {
        this.point1 = Vector3.zero;
        this.point2 = Vector3.zero;

        _k = 0f;
        _b = 0f;

        _point1 = point1;
        _point2 = point2;

        GetStandartForm(_point1, _point2);
    }

    public void GetStandartForm(Vector3 point1, Vector3 point2)
    {
        _k = (point1.z - point2.z) / (point1.x - point2.x);
        _b = point2.z - point2.x * _k;
    }

    /*public Vector3 Rotate(float radius, float angle, Vector3 center)
    {
        Vector3 point = new Vector3
        {
            x = radius * Mathf.Sin(angle * Mathf.Deg2Rad),
            z = radius * Mathf.Cos(angle * Mathf.Deg2Rad)
        };

        return center + point;
    }*/

  /*  /// <summary>
    /// поворот прямой на угол angle
    /// </summary>
    /// <param name="angle">угол в градусах</param>
    /// <returns></returns>
    public Line Rotate( float angleDegrees )
    {
        Line line = new Line( this._point1, this._point2); 

        float angleRad = - angleDegrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(angleRad), cos = Mathf.Cos(angleRad);




        line.point2.x = line._point2.x * cos - line._point2.z * sin;
        line.point2.z = line._point2.x * sin + line._point2.z * cos;

        line._point1 = line.point1;
        line._point2 = line.point2;

        return line;
    }*/
}
