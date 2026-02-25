using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class SniperJoeBullet2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifetime = 3f;

    [Header("Collision")]
    [SerializeField] private LayerMask hitMask; // Player + Ground (whatever should destroy the bullet)

    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
    }

    // Call this ONCE when spawned.
    // targetPos is the player's position at the moment of firing.
    public void FireToward(Vector2 origin, Vector2 targetPos)
    {
        Vector2 dir = (targetPos - origin);
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.right; // fallback to avoid zero direction

        dir.Normalize();

        // If kinematic: set velocity anyway (Unity still stores it), or you can MovePosition in FixedUpdate.
        _rb.linearVelocity = dir * speed;


        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Layer-mask check
        if (((1 << other.gameObject.layer) & hitMask) == 0)
            return;

        // TODO later: if other is Player -> damage
        // For now: just destroy on hit with player/ground/etc.
        Destroy(gameObject);
    }
}
