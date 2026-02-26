using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BlasterTurret2D : MonoBehaviour
{
    public enum BlasterType { RedShotgun, BlueMachineGun, YellowSniper }

    [Header("Type")]
    [SerializeField] private BlasterType type;

    [Header("Explicit refs (avoid flipping wrong renderer)")]
    [SerializeField] private SpriteRenderer sr;   // drag the visible sprite renderer here
    [SerializeField] private Animator anim;       // optional
    [SerializeField] private Transform firePoint; // required
    [SerializeField] private BlasterBullet2D bulletPrefab; // required (color-specific bullet)

    [Header("Targeting")]
    [Tooltip("Optional. Leave null to auto-target closest PlayerMovement2D (spawn-safe).")]
    [SerializeField] private Transform player;
    [SerializeField] private bool facePlayer = true;     // if false, turret uses its current facing
    [SerializeField] private float aimMaxDistance = 9999f;

    [Header("Fire rules")]
    [SerializeField] private bool onlyFireOnScreen = true;
    [SerializeField] private float attackCooldown = 1.2f;

    [Header("Red (Shotgun forward)")]
    [SerializeField] private int shotgunPellets = 5;
    [SerializeField] private float shotgunSpeed = 7f;
    [SerializeField] private float shotgunSpreadDegrees = 30f;

    [Header("Blue (Machine gun volley toward player)")]
    [SerializeField] private int volleyShots = 6;
    [SerializeField] private float volleyShotInterval = 0.08f;
    [SerializeField] private float volleySpeed = 8f;
    [SerializeField] private float volleyInaccuracyDegrees = 10f;

    [Header("Yellow (Sniper single fast shot)")]
    [SerializeField] private float sniperSpeed = 13f;

    [Header("Debug")]
    [SerializeField] private bool debug = false;
    [SerializeField] private bool drawAim = false;

    public enum WallSide { LeftWall, RightWall }

    [Header("Placement")]
    [SerializeField] private WallSide wallSide = WallSide.RightWall; // default faces LEFT
    [SerializeField] private bool applyWallSideOnAwake = true;

    private bool _isVisible;


    private void Awake()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (anim == null) anim = GetComponent<Animator>();

        if (applyWallSideOnAwake)
            ApplyWallSideFacing();
    }

    private void OnBecameVisible() => _isVisible = true;
    private void OnBecameInvisible() => _isVisible = false;

    private void Start()
    {
        StartCoroutine(FireLoop());
    }

    // LateUpdate so Animator can't undo flipX if clips/keyframes exist
    private void LateUpdate()
    {
        AcquireClosestPlayerIfNeeded();

        if (facePlayer)
            FacePlayerNow(); // keep facing correct even if player crosses sides
    }

    private void AcquireClosestPlayerIfNeeded()
    {
        if (player != null && player.gameObject.activeInHierarchy) return;

        PlayerMovement2D[] candidates = FindObjectsByType<PlayerMovement2D>(FindObjectsSortMode.None);
        if (candidates == null || candidates.Length == 0) return;

        float best = float.PositiveInfinity;
        Transform bestT = null;
        Vector2 me = transform.position;

        foreach (var c in candidates)
        {
            if (c == null || !c.gameObject.activeInHierarchy) continue;
            float d = Vector2.SqrMagnitude((Vector2)c.transform.position - me);
            if (d < best)
            {
                best = d;
                bestT = c.transform;
            }
        }

        if (bestT != null && bestT != player)
        {
            player = bestT;
            if (debug) Debug.Log($"[Blaster] Target acquired: {player.name}");
        }
    }

    private void FacePlayerNow()
    {
        if (player == null || sr == null) return;

        float dx = player.position.x - transform.position.x;
        if (Mathf.Abs(dx) > aimMaxDistance) return;

        bool playerIsRight = dx > 0f;

        // You said: ALL sprites face LEFT by default.
        // flipX true => face RIGHT
        sr.flipX = playerIsRight;

        // Mirror firePoint with facing
        if (firePoint != null)
        {
            Vector3 lp = firePoint.localPosition;
            lp.x = Mathf.Abs(lp.x) * (playerIsRight ? 1f : -1f);
            firePoint.localPosition = lp;
        }

        if (debug)
            Debug.Log($"[Blaster] dx={dx:F2} flipX={sr.flipX} player={player.name}");
    }

    private IEnumerator FireLoop()
    {
        while (true)
        {
            // Cooldown between attack cycles
            yield return new WaitForSeconds(attackCooldown);

            // Spawn-safe: keep reacquiring player if needed
            AcquireClosestPlayerIfNeeded();

            // Don’t shoot offscreen (Mega Man fairness)
            if (onlyFireOnScreen && !_isVisible) continue;

            // Must have refs
            if (player == null || bulletPrefab == null || firePoint == null || sr == null) continue;

            // Optional range gate
            float dx = player.position.x - transform.position.x;
            if (Mathf.Abs(dx) > aimMaxDistance) continue;

            // Ensure facing is correct at fire time
            if (facePlayer)
                FacePlayerNow();

            // ---- OPEN (vulnerable) ----
            SetOpen(true);

            // Telegraphed vulnerable window BEFORE first shot
            // (Tune per type if you want different feels)
            float pre = type switch
            {
                BlasterType.RedShotgun => 0.25f,
                BlasterType.BlueMachineGun => 0.20f,
                BlasterType.YellowSniper => 0.30f,
                _ => 0.25f
            };
            yield return new WaitForSeconds(pre);

            // ---- FIRE PATTERN ----
            switch (type)
            {
                case BlasterType.RedShotgun:
                    if (anim != null) anim.SetTrigger("shoot"); // once per blast
                    FireShotgunForward();
                    break;

                case BlasterType.BlueMachineGun:
                    if (anim != null) anim.SetTrigger("shoot"); // once per volley
                    yield return FireVolleyTowardPlayer();       // volleyShots * interval inside
                    break;

                case BlasterType.YellowSniper:
                    if (anim != null) anim.SetTrigger("shoot"); // once per shot
                    FireSniperTowardPlayer();
                    break;
            }

            // Vulnerable window AFTER firing
            float post = type switch
            {
                BlasterType.RedShotgun => 0.25f,
                BlasterType.BlueMachineGun => 0.20f,
                BlasterType.YellowSniper => 0.35f,
                _ => 0.25f
            };
            yield return new WaitForSeconds(post);

            // ---- CLOSE (shielded) ----
            SetOpen(false);
        }
    }

    // ---- Patterns ----

    // Shotgun: cone spread in turret's forward direction (based on current flipX)
    private void FireShotgunForward()
    {
        Vector2 origin = firePoint.position;
        Vector2 forward = sr.flipX ? Vector2.right : Vector2.left;

        if (drawAim) Debug.DrawRay(origin, forward * 2f, Color.red, 0.5f);

        for (int i = 0; i < shotgunPellets; i++)
        {
            float t = (shotgunPellets == 1) ? 0f : (i / (shotgunPellets - 1f));
            float ang = Mathf.Lerp(-shotgunSpreadDegrees * 0.5f, shotgunSpreadDegrees * 0.5f, t);

            Vector2 dir = Rotate(forward, ang);

            BlasterBullet2D b = Instantiate(bulletPrefab, origin, Quaternion.identity);
            b.Fire(dir, shotgunSpeed);
        }
    }

    // Machine gun: volley aimed generally toward player center mass with inaccuracy
    private IEnumerator FireVolleyTowardPlayer()
    {
        for (int i = 0; i < volleyShots; i++)
        {
            AcquireClosestPlayerIfNeeded();
            if (player == null) yield break;
            if (onlyFireOnScreen && !_isVisible) yield break;

            Vector2 origin = firePoint.position;
            Vector2 target = GetPlayerCenterMass();
            Vector2 dir = (target - origin);
            if (dir.sqrMagnitude < 0.0001f) dir = sr.flipX ? Vector2.right : Vector2.left;
            dir.Normalize();

            float randomAngle = Random.Range(-volleyInaccuracyDegrees, volleyInaccuracyDegrees);
            dir = Rotate(dir, randomAngle);

            if (drawAim) Debug.DrawLine(origin, origin + dir * 2f, Color.cyan, 0.25f);

            BlasterBullet2D b = Instantiate(bulletPrefab, origin, Quaternion.identity);
            b.Fire(dir, volleySpeed);

            yield return new WaitForSeconds(volleyShotInterval);
        }
    }

    // Sniper: single fast accurate shot at player center mass
    private void FireSniperTowardPlayer()
    {
        Vector2 origin = firePoint.position;
        Vector2 target = GetPlayerCenterMass();
        Vector2 dir = (target - origin);

        if (dir.sqrMagnitude < 0.0001f)
            dir = sr.flipX ? Vector2.right : Vector2.left;

        dir.Normalize();

        if (drawAim) Debug.DrawLine(origin, target, Color.yellow, 0.75f);

        BlasterBullet2D b = Instantiate(bulletPrefab, origin, Quaternion.identity);
        b.Fire(dir, sniperSpeed);
    }

    // ---- Helpers ----

    private Vector2 GetPlayerCenterMass()
    {
        if (player == null) return Vector2.zero;

        Collider2D pc = player.GetComponent<Collider2D>();
        if (pc != null) return pc.bounds.center;

        SpriteRenderer psr = player.GetComponent<SpriteRenderer>();
        if (psr != null) return psr.bounds.center;

        return player.position;
    }

    private static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }

    private void SetOpen(bool open)
    {
        if (anim != null) anim.SetBool("isOpen", open);
    }

    private void ApplyWallSideFacing()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        // sprites face LEFT by default:
        // flipX true => face RIGHT
        bool faceRight = (wallSide == WallSide.LeftWall);
        sr.flipX = faceRight;

        if (firePoint != null)
        {
            Vector3 lp = firePoint.localPosition;
            lp.x = Mathf.Abs(lp.x) * (faceRight ? 1f : -1f);
            firePoint.localPosition = lp;
        }
    }
}