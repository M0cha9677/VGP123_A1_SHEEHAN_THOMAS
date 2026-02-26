using UnityEngine;

public class HazaedDamage2D : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var stats = other.GetComponent<PlayerStats2D>();
        if (stats == null) return;

        stats.TakeDamage(damage);
    }
}
