using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyProjectile2D : MonoBehaviour
{
    [SerializeField] private LayerMask hitMask;

    private Rigidbody2D _rb;
    private int _damage;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
    }

    public void Fire(Vector2 direction, float speed, int damage, float lifetime)
    {
        _damage = damage;
        _rb.linearVelocity = direction.normalized * speed;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerRecieveDamage receiver = other.GetComponent<PlayerRecieveDamage>();
        if (receiver != null)
        {
            receiver.ApplyDamage(_damage);
            Destroy(gameObject);
            return;
        }

        if (((1 << other.gameObject.layer) & hitMask) != 0)
            Destroy(gameObject);
    }
}