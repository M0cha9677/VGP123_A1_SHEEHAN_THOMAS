using UnityEngine;

public class Pickup_Energy : MonoBehaviour
{
    [SerializeField] private int energyAmount = 5;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var stats = other.GetComponent<PlayerStats2D>();
        if (stats == null) return;

        stats.AddEnergy(energyAmount);
        Destroy(gameObject);
    }
}
