using UnityEngine;

public class Pickup_OneUp : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        var stats = other.GetComponent<PlayerStats2D>();
        if (stats == null) return;

        stats.AddLife(1);
        Destroy(gameObject);
    }
}
