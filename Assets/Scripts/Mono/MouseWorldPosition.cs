using UnityEngine;

public class MouseWorldPosition : MonoBehaviour
{
    public static Vector3 GetMouseWorldPosition()
    {
        Ray mouseCameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up,Vector3.zero);
        if (plane.Raycast(mouseCameraRay, out float distance))
        {
            return mouseCameraRay.GetPoint(distance);
        }
        else
        {
            return Vector3.zero;
        };
    }
}
