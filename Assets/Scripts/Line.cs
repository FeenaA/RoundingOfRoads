using UnityEngine;

public struct Line
{
    private float _k;
    private float _bb;

    public Vector3 point1;
    public Vector3 point2;

    private Vector3 _point1;
    private Vector3 _point2;

    private float _a;
    private float _b;
    private float _c;

    public float k
    {
        get { return _k; }
        set { _k = value; }
    }

    public float bb
    {
        get { return _bb; }
        set { _bb = value; }
    }

    public float a
    {
        get { return _a; }
        set { _a = value; }
    }

    public float b
    {
        get { return _b; }
        set { _b = value; }
    }

    public float c
    {
        get { return _c; }
        set { _c = value; } 
    }

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

        _a = _b = _c = 0;

        _k = k;
        _bb = b;

        // GetTwoPointsForm();
    }

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
        _bb = 0f;

        _a = _b = _c = 0;

        _point1 = point1;
        _point2 = point2;

        GetGeneralForm(_point1, _point2);
    }

    public void GetGeneralForm(Vector3 point1, Vector3 point2)
    {
        _k = (point1.z - point2.z) / (point1.x - point2.x);
        _bb = point2.z - point2.x * _k;

        // Я
        /*_a = point2.x - point1.x;
        _b = point1.z - point2.z;
        _c = - point1.z * _a - point1.x * _b;*/

        // Даниил
        /*_a = point2.z - point1.z;
        _b = point1.x - point2.x;  
        _c = point1.x * point2.z - point2.x * point1.z;*/

        _a = point1.z - point2.z;
        _b = point2.x - point1.x;
        _c = point1.x * point2.z - point2.x * point1.z;

    }
}
