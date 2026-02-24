using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 10f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.05f;
    [SerializeField] private float groundCheckExtra = 0.02f;

    [Header("Optional FirePoint")]
    [SerializeField] private Transform firePoint;

    [Header("Level Clamp")]
    [SerializeField] private SpriteRenderer levelSprite;

    public bool FacingRight => _sr.flipX;

    private Rigidbody2D _rb;
    private Collider2D _col;
    private SpriteRenderer _sr;
    private Animator _anim;

    private float _firePointAbsX;
    private float _moveInput;
    private bool _jumpPressed;
    private bool _isGrounded;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _sr = GetComponent<SpriteRenderer>();
        _anim = GetComponent<Animator>();

        if (firePoint != null)
            _firePointAbsX = Mathf.Abs(firePoint.localPosition.x);
    }

    private void Start()
    {
        // Auto-find level sprite if not assigned (prefab-safe)
        if (levelSprite == null)
        {
            GameObject levelObj = GameObject.FindGameObjectWithTag("Level");
            if (levelObj != null)
                levelSprite = levelObj.GetComponent<SpriteRenderer>();
            else
                Debug.LogError("No GameObject with tag 'Level' found.");
        }
    }

    private void Update()
    {
        // Input
        _moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
            _jumpPressed = true;

        // Flip sprite
        if (_moveInput != 0f)
            SpriteFlip(_moveInput);

        // Ground check
        _isGrounded = Physics2D.OverlapCircle(
            GetGroundCheckPos(),
            groundCheckRadius,
            groundLayer
        );

        // Animator parameters
        _anim.SetFloat("moveInput", Mathf.Abs(_moveInput));
        _anim.SetBool("isGrounded", _isGrounded);
        _anim.SetFloat("yVel", _rb.linearVelocity.y);
    }

    private void FixedUpdate()
    {
        // Horizontal movement
        Vector2 velocity = _rb.linearVelocity;
        velocity.x = _moveInput * moveSpeed;
        _rb.linearVelocity = velocity;

        // Jump
        if (_jumpPressed && _isGrounded)
        {
            velocity = _rb.linearVelocity;
            velocity.y = 0f;
            _rb.linearVelocity = velocity;

            _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        _jumpPressed = false;

        ClampToLevel();
    }

    private Vector2 GetGroundCheckPos()
    {
        Bounds b = _col.bounds;
        return new Vector2(b.center.x, b.min.y - groundCheckExtra);
    }

    private void SpriteFlip(float horizontalInput)
    {
        if (horizontalInput == 0) return;
        _sr.flipX = horizontalInput > 0f;

        if (firePoint != null)
        {
            Vector3 p = firePoint.localPosition;
            p.x = _sr.flipX ? -_firePointAbsX : _firePointAbsX;
            firePoint.localPosition = p;
        }
    }

    private void ClampToLevel()
    {
        if (levelSprite == null) return;

        Bounds b = levelSprite.bounds;
        Vector3 pos = transform.position;

        // Clamp left/right
        pos.x = Mathf.Clamp(pos.x, b.min.x, b.max.x);

        // Clamp top only (allow falling off bottom if desired)
        pos.y = Mathf.Min(pos.y, b.max.y);

        transform.position = pos;
    }

    private void OnDrawGizmosSelected()
    {
        if (_col == null) return;

        Bounds b = _col.bounds;
        Vector3 pos = new Vector3(b.center.x, b.min.y - groundCheckExtra, 0f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pos, groundCheckRadius);
    }
}


