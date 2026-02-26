using UnityEngine;

[RequireComponent(typeof(PlayerStats2D))]
public class PlayerRecieveDamage : MonoBehaviour
{

    private PlayerStats2D _stats;
    void Awake()
    {
        _stats = GetComponent<PlayerStats2D>();
    }

    public void ApplyDamage (int amount)
    {
        if (_stats != null)
            _stats.TakeDamage(amount);
    }
}
