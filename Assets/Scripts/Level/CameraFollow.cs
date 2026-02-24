using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private SpriteRenderer levelSprite;
    [SerializeField] private float smoothSpeed = 5f;

    private float minX, maxX, minY, maxY;
    private float camHalfWidth, camHalfHeight;

    private void Start()
    {
        if (levelSprite == null)
        {
            Debug.LogError("Level sprite not assigned.");
            return;
        }

        camHalfHeight = Camera.main.orthographicSize;
        camHalfWidth = camHalfHeight * Camera.main.aspect;

        Bounds b = levelSprite.bounds;

        minX = b.min.x + camHalfWidth;
        maxX = b.max.x - camHalfWidth;
        minY = b.min.y + camHalfHeight;
        maxY = b.max.y - camHalfHeight;
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = new Vector3(
            target.position.x,
            target.position.y,
            transform.position.z
        );

        float clampedX = Mathf.Clamp(desired.x, minX, maxX);
        float clampedY = Mathf.Clamp(desired.y, minY, maxY);

        Vector3 clampedPos = new Vector3(clampedX, clampedY, desired.z);

        transform.position = Vector3.Lerp(
            transform.position,
            clampedPos,
            smoothSpeed * Time.deltaTime
        );
    }
}
