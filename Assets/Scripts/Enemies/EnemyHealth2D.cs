using UnityEngine;

public class EnemyHealth2D : MonoBehaviour
{
    [SerializeField] private int maxHP = 3;
    [SerializeField] private GameObject deathFXPrefab;

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
        if (deathFXPrefab != null)
            Instantiate(deathFXPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}

