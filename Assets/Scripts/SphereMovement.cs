using UnityEngine;
using UnityEngine.Events;

public class SphereMovement : MonoBehaviour
{
    public Plane plane = new Plane(Vector3.up, 0f);

    public event UnityAction PositionChanged;

    /// <summary>
    /// зажата сфера
    /// </summary>
    public void OnMouseDrag() 
    {
        //Detect when there is a mouse click
        if (Input.GetMouseButton(0))
        {
            //Create a ray from the Mouse click position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            //Initialise the enter variable
            float enter = 0.0f;

            if (plane.Raycast(ray, out enter))
            {
                //Get the point that is clicked
                Vector3 hitPoint = ray.GetPoint(enter);

                //Move your cube GameObject to the point where you clicked
                transform.position = hitPoint;

                PositionChanged?.Invoke();
            }
        }
    }
}
