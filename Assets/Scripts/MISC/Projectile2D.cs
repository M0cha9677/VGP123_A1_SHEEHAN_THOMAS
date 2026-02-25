using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile2D : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private int damage = 1;

    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
    }

    public void Fire(Vector2 direction)
    {
        _rb.linearVelocity = direction.normalized * speed;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Level")) return;

        BlasterHealth2D blaster = other.GetComponentInParent<BlasterHealth2D>();
        if (blaster != null) 
        {
            blaster.TakeDamage(damage);
            Destroy(gameObject);
            return;

        }
        // Add sniper joe and Razy here in the same way
        Destroy(gameObject);
    }
    
}
