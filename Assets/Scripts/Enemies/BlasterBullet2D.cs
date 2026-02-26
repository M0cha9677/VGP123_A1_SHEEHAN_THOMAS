using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BlasterBullet2D : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private LayerMask hitMask;

    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
    }

    public void Fire(Vector2 direction, float speed)
    {
        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.right;

        direction.Normalize();
        _rb.linearVelocity = direction * speed;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var receiver = other.GetComponent<PlayerRecieveDamage>();
        if (receiver != null)
        {
            receiver.ApplyDamage(1);
            Destroy(gameObject);
            return;
        }

        if (((1 << other.gameObject.layer) & hitMask) == 0)
            return;

        Destroy(gameObject);
    }
}