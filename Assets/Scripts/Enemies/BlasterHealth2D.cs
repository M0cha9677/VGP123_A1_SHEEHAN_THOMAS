using UnityEngine;

public class BlasterHealth2D : MonoBehaviour
{
    [SerializeField] private int maxHP = 3;

    private int _hp;

    private void Awake()
    {
        _hp = maxHP;
    }

    public void TakeDamage(int amount)
    {
        _hp -= amount;
        if (_hp <= 0)
            Die();
    }

    private void Die()
    {
        // later: play anim, spawn particles, drop item, etc.
        Destroy(gameObject);
    }
}
