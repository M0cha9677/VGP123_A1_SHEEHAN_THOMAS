using UnityEngine;

public class Pickup_Health : MonoBehaviour
{
    [Header("Health Restore")]
    [SerializeField] private int healAmount = 3;
    // Set to 10 if you want full heal

    private void OnTriggerEnter2D(Collider2D other)
    {
        var stats = other.GetComponent<PlayerStats2D>();
        if (stats == null) return;

        stats.Heal(healAmount);

        Destroy(gameObject);
    }
}
