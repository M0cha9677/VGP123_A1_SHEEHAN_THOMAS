using UnityEngine;

public class CameraSectionTrigger : MonoBehaviour
{
    [Header("Section Bounds")]
    public float left;
    public float right;
    public float bottom;
    public float top;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        CameraFollow2D camFollow = Camera.main.GetComponent<CameraFollow2D>();
        if (camFollow == null) return;

        Camera cam = Camera.main;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        float minX = left + halfW;
        float maxX = right - halfW;
        float minY = bottom + halfH;
        float maxY = top - halfH;

        camFollow.SetSectionBounds(minX, maxX, minY, maxY);
    }
}
