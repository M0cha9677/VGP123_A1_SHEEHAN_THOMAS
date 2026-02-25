using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(SpriteRenderer))]
public class EnemyPatrol2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Ground Check (Player-style)")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.05f;
    [SerializeField] private float groundCheckExtra = 0.02f;

    [Header("Wall Check")]
    [SerializeField] private float wallCheckDistance = 0.15f;
    [SerializeField] private float wallCheckHeightOffset = 0.0f;

    [Header("Turn Control")]
    [SerializeField] private float turnCooldown = 0.1f;

    private Rigidbody2D _rb;
    private Collider2D _col;
    private SpriteRenderer _sr;

    private int _dir = -1; // -1 = left, +1 = right
    private float _nextTurnTime;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _sr = GetComponent<SpriteRenderer>();

        // Optional: set initial sprite flip to match direction
        _sr.flipX = (_dir == 1);
    }

    private void FixedUpdate()
    {
        // Move constantly
        Vector2 vel = _rb.linearVelocity;
        vel.x = _dir * moveSpeed;
        _rb.linearVelocity = vel;

        if (Time.time < _nextTurnTime) return;

        bool wallAhead = CheckWallAhead();
        bool groundAhead = CheckGroundAhead(); // edge detection

        if (wallAhead || !groundAhead)
            TurnAround();
    }

    // ----- Checks -----

    // Similar to your player: OverlapCircle at a computed point under the collider.
    // Difference: we put it at the FRONT FOOT (ahead in the movement direction) so it detects edges.
    private bool CheckGroundAhead()
    {
        Vector2 pos = GetFrontGroundCheckPos();
        return Physics2D.OverlapCircle(pos, groundCheckRadius, groundLayer);
    }

    private Vector2 GetFrontGroundCheckPos()
    {
        Bounds b = _col.bounds;

        float x = (_dir == 1) ? b.max.x : b.min.x; // front edge of collider
        float y = b.min.y - groundCheckExtra;      // slightly below feet (same idea as player)

        return new Vector2(x, y);
    }

    private bool CheckWallAhead()
    {
        Bounds b = _col.bounds;

        // Cast forward from the front edge at mid-height (or tweak height offset)
        float x = (_dir == 1) ? b.max.x : b.min.x;
        float y = b.center.y + wallCheckHeightOffset;

        Vector2 origin = new Vector2(x, y);

        return Physics2D.Raycast(origin, Vector2.right * _dir, wallCheckDistance, groundLayer);
    }

    // ----- Turn -----

    private void TurnAround()
    {
        _dir *= -1;
        _nextTurnTime = Time.time + turnCooldown;

        // Match your player style: use SpriteRenderer.flipX
        _sr.flipX = (_dir == 1);
    }

    // ----- Debug -----

    private void OnDrawGizmosSelected()
    {
        // Draw ground overlap circle + wall ray
        Collider2D c = _col != null ? _col : GetComponent<Collider2D>();
        if (c == null) return;

        Bounds b = c.bounds;

        int dir = Application.isPlaying ? _dir : -1;

        Vector3 groundPos = new Vector3(
            (dir == 1) ? b.max.x : b.min.x,
            b.min.y - groundCheckExtra,
            0f
        );

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundPos, groundCheckRadius);

        Vector3 wallOrigin = new Vector3(
            (dir == 1) ? b.max.x : b.min.x,
            b.center.y + wallCheckHeightOffset,
            0f
        );

        Gizmos.DrawLine(wallOrigin, wallOrigin + (Vector3)(Vector2.right * dir * wallCheckDistance));
    }
}
