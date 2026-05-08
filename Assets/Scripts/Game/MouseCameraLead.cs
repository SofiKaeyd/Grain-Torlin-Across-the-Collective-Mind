using UnityEngine;

public class MouseCameraLead : MonoBehaviour
{
    [SerializeField] private float _maxRadius = 3f;

    void Update()
    {
        Camera cam = MouseManager.ActiveCamera;
        if (cam == null) return;

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        Vector3 playerPos = Player.Instance.transform.position;
        Vector3 direction = mousePos - playerPos;

        if (direction.sqrMagnitude > _maxRadius * _maxRadius) // sqrMagnitude для скорости
        {
            direction = direction.normalized * _maxRadius;
        }

        transform.position = playerPos + direction;
    }
}