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
}
