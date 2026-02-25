using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 6f;

    private float _minX, _maxX, _minY, _maxY;

    public void SetSectionBounds(float minX, float maxX, float minY, float maxY)
    {
        _minX = minX;
        _maxX = maxX;
        _minY = minY;
        _maxY = maxY;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 current = transform.position;
        Vector3 desired = new Vector3(target.position.x, target.position.y, current.z);

        float x = Mathf.Clamp(desired.x, _minX, _maxX);
        float y = Mathf.Clamp(desired.y, _minY, _maxY);

        Vector3 clamped = new Vector3(x, y, desired.z);

        transform.position = Vector3.Lerp(current, clamped, smoothSpeed * Time.deltaTime);
    }

    public void SetTarget(Transform t) => target = t;
}
