using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class SniperJoe2D : MonoBehaviour
{
    public enum State { ShieldUp, ShieldDownShoot }

    [Header("Render / Anim (explicit)")]
    [Tooltip("Drag the SpriteRenderer that is actually visible for Joe (avoid flipping the wrong renderer).")]
    [SerializeField] private SpriteRenderer sr;
    [Tooltip("Optional. Drag Joe's Animator if you have one.")]
    [SerializeField] private Animator anim;

    [Header("Player Targeting")]
    [Tooltip("Optional. If empty, Joe will auto-target the closest active PlayerMovement2D every frame.")]
    [SerializeField] private Transform player;
    [SerializeField] private float facePlayerMaxDistance = 9999f;

    [Header("Timing (MM1 feel)")]
    [SerializeField] private float shieldUpTime = 1.0f;
    [SerializeField] private float shieldDownPreShotTime = 0.15f;
    [SerializeField] private float shieldDownPostShotTime = 0.25f;

    [Header("Shooting")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private SniperJoeBullet2D joeBulletPrefab;

    [Header("On-Screen Gate")]
    [SerializeField] private bool onlyShootOnGameCamera = true;

    [Header("Shield (later)")]
    [SerializeField] private bool frontOnlyShieldBlock = true;

    [Header("Debug")]
    [SerializeField] private bool drawAimLine = true;

    private Rigidbody2D _rb;
    private Collider2D _col;

    private State _state = State.ShieldUp;
    private Coroutine _loop;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();

        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (anim == null) anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (_loop == null)
            _loop = StartCoroutine(StateLoop());
    }

    private void OnDisable()
    {
        if (_loop != null)
        {
            StopCoroutine(_loop);
            _loop = null;
        }
    }

    private void LateUpdate()
    {
        AcquireClosestPlayerMovement();
        FacePlayer();
    }

    private void AcquireClosestPlayerMovement()
    {
        if (player != null && player.gameObject.activeInHierarchy) return;

        PlayerMovement2D[] candidates = FindObjectsByType<PlayerMovement2D>(FindObjectsSortMode.None);
        if (candidates == null || candidates.Length == 0) return;

        float bestDist = float.PositiveInfinity;
        Transform best = null;

        Vector2 myPos = transform.position;

        foreach (var c in candidates)
        {
            if (c == null || !c.gameObject.activeInHierarchy) continue;

            float d = Vector2.SqrMagnitude((Vector2)c.transform.position - myPos);
            if (d < bestDist)
            {
                bestDist = d;
                best = c.transform;
            }
        }

        if (best != null && best != player)
            player = best;
    }

    private void FacePlayer()
    {
        if (player == null || sr == null) return;

        float dx = player.position.x - transform.position.x;
        if (Mathf.Abs(dx) > facePlayerMaxDistance) return;

        bool playerIsRight = dx > 0f;

        // All sprites face LEFT by default:
        // flipX = true means face RIGHT
        sr.flipX = playerIsRight;

        if (firePoint != null)
        {
            Vector3 lp = firePoint.localPosition;
            lp.x = Mathf.Abs(lp.x) * (playerIsRight ? 1f : -1f);
            firePoint.localPosition = lp;
        }
    }

    private IEnumerator StateLoop()
    {
        while (true)
        {
            SetState(State.ShieldUp);
            yield return new WaitForSeconds(shieldUpTime);

            SetState(State.ShieldDownShoot);
            yield return new WaitForSeconds(shieldDownPreShotTime);

            ShootOnce();

            yield return new WaitForSeconds(shieldDownPostShotTime);
        }
    }

    private void SetState(State s)
    {
        _state = s;

        if (anim != null)
            anim.SetBool("shieldUp", _state == State.ShieldUp);
    }

    private void ShootOnce()
    {
        // IMPORTANT: Use Game camera check, not OnBecameVisible (Scene view lies in editor)
        if (onlyShootOnGameCamera && !IsRendererOnGameCamera())
            return;

        AcquireClosestPlayerMovement();

        if (joeBulletPrefab == null || firePoint == null) return;
        if (player == null) return;

        FacePlayer();

        Vector2 origin = firePoint.position;
        Vector2 targetSnapshot = GetPlayerCenterMass();

        if (drawAimLine)
            Debug.DrawLine(origin, targetSnapshot, Color.red, 1.0f);

        SniperJoeBullet2D bullet = Instantiate(joeBulletPrefab, origin, Quaternion.identity);
        bullet.FireToward(origin, targetSnapshot);

        if (anim != null)
            anim.SetTrigger("shoot");
    }

    public bool CanTakeDamageFrom(Vector2 attackerPosition)
    {
        if (_state != State.ShieldUp) return true;
        if (!frontOnlyShieldBlock) return false;

        bool facingRight = sr != null && sr.flipX;
        float dx = attackerPosition.x - transform.position.x;
        bool attackerInFront = facingRight ? (dx > 0f) : (dx < 0f);
        return !attackerInFront;
    }

    public bool IsShieldUp => _state == State.ShieldUp;

    private Vector2 GetPlayerCenterMass()
    {
        if (player == null) return Vector2.zero;

        Collider2D pc = player.GetComponent<Collider2D>();
        if (pc != null) return pc.bounds.center;

        SpriteRenderer psr = player.GetComponent<SpriteRenderer>();
        if (psr != null) return psr.bounds.center;

        return player.position;
    }

    // More accurate than pivot viewport check (uses sprite bounds)
    private bool IsRendererOnGameCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return true; // fail-open (build safety)

        if (sr == null) return IsOnGameCameraPivotOnly();

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        return GeometryUtility.TestPlanesAABB(planes, sr.bounds);
    }

    // Fallback if sr is missing
    private bool IsOnGameCameraPivotOnly()
    {
        Camera cam = Camera.main;
        if (cam == null) return true;

        Vector3 vp = cam.WorldToViewportPoint(transform.position);
        return (vp.z > 0f && vp.x > 0f && vp.x < 1f && vp.y > 0f && vp.y < 1f);
    }
}