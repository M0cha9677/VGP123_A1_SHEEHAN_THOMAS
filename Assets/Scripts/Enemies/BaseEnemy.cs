using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent (typeof(Collider2D))]
public class BaseEnemy : MonoBehaviour
{

    [Header("Core Stats")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private EnemyProjectile2D projectilePrefab;
    [SerializeField] private int projectileDamage = 1;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float projectileLifetime = 3f;
    [SerializeField] private int contactDamage = 1;
    [SerializeField] private bool usesProjectiles = false;

    [Header("Effects")]
    [SerializeField] private GameObject deathFXPrefab;
    [SerializeField] private Animator anim;

    private int _currentHealth;
    private bool _dead;

    public int ContactDamage => contactDamage;
    public bool UsesProjectiles => usesProjectiles;
    public int ProjectileDamage => projectileDamage;
    public float ProjectileSpeed => projectileSpeed;
    public float ProjectileLifetime => projectileLifetime;
    public bool IsDead => _dead;

    private void Awake()
    {
        _currentHealth = maxHealth;

        if (anim == null)
            anim.GetComponentInChildren<Animator>();
    }

    public void TakeDamage(int amount)
    {
        if (_dead || amount <= 0) return;

        _currentHealth -= amount;

        if (_currentHealth <= 0)
        {
            Die();
            return;
        }

        if (anim != null)
            anim.SetTrigger("hurt");
    }

    public void SpawnProjectile(Vector2 origin, Vector2 direction)
    {
        if (_dead || !usesProjectiles || projectilePrefab == null) return;

        EnemyProjectile2D proj = Instantiate(projectilePrefab, origin, Quaternion.identity);
        proj.Fire(direction, projectileSpeed, projectileDamage, projectileLifetime);
    }

    protected virtual void Die()
    {
        _dead = true;

        if (deathFXPrefab != null)
            Instantiate(deathFXPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
