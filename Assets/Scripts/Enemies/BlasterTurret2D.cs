using System.Collections;
using UnityEngine;

public class BlasterTurret2D : MonoBehaviour
{
    public enum BlasterType { RedShotgun, BlueMachineGun, YellowSniper }
    public enum WallSide { LeftWall, RightWall }

    [Header("Type")]
    [SerializeField] private BlasterType type;

    [Header("Explicit refs (avoid flipping wrong renderer)")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Animator anim;
    [SerializeField] private Transform firePoint;
    [SerializeField] private BlasterBullet2D bulletPrefab;

    [Header("Targeting")]
    [Tooltip("Optional. Leave null to auto-target closest PlayerMovement2D (spawn-safe).")]
    [SerializeField] private Transform player;
    [SerializeField] private bool facePlayer = true;
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

    [Header("Placement")]
    [SerializeField] private WallSide wallSide = WallSide.RightWall; // default faces LEFT
    [SerializeField] private bool applyWallSideOnAwake = true;

    [Header("Debug")]
    [SerializeField] private bool debug = false;
    [SerializeField] private bool drawAim = false;

    private void Awake()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (anim == null) anim = GetComponent<Animator>();

        if (applyWallSideOnAwake && !facePlayer)
            ApplyWallSideFacing();
    }

    private void Start()
    {
        StartCoroutine(FireLoop());
    }

    private void LateUpdate()
    {
        AcquireClosestPlayerIfNeeded();

        if (facePlayer)
            FacePlayerNow();
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

        // All sprites face LEFT by default -> flipX true means face RIGHT
        sr.flipX = playerIsRight;

        if (firePoint != null)
        {
            Vector3 lp = firePoint.localPosition;
            lp.x = Mathf.Abs(lp.x) * (playerIsRight ? 1f : -1f);
            firePoint.localPosition = lp;
        }
    }

    private IEnumerator FireLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(attackCooldown);

            AcquireClosestPlayerIfNeeded();

            // Use GAME camera visibility check (Scene view won't affect this)
            if (onlyFireOnScreen && !IsRendererOnGameCamera())
                continue;

            if (player == null || bulletPrefab == null || firePoint == null || sr == null) continue;

            float dx = player.position.x - transform.position.x;
            if (Mathf.Abs(dx) > aimMaxDistance) continue;

            if (facePlayer)
                FacePlayerNow();

            SetOpen(true);

            float pre = type switch
            {
                BlasterType.RedShotgun => 0.25f,
                BlasterType.BlueMachineGun => 0.20f,
                BlasterType.YellowSniper => 0.30f,
                _ => 0.25f
            };
            yield return new WaitForSeconds(pre);

            // Fire pattern
            switch (type)
            {
                case BlasterType.RedShotgun:
                    if (anim != null) anim.SetTrigger("shoot");
                    FireShotgunForward();
                    break;

                case BlasterType.BlueMachineGun:
                    if (anim != null) anim.SetTrigger("shoot");
                    yield return FireVolleyTowardPlayer(); // includes mid-volley on-screen checks
                    break;

                case BlasterType.YellowSniper:
                    if (anim != null) anim.SetTrigger("shoot");
                    FireSniperTowardPlayer();
                    break;
            }

            float post = type switch
            {
                BlasterType.RedShotgun => 0.25f,
                BlasterType.BlueMachineGun => 0.20f,
                BlasterType.YellowSniper => 0.35f,
                _ => 0.25f
            };
            yield return new WaitForSeconds(post);

            SetOpen(false);
        }
    }

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

    private IEnumerator FireVolleyTowardPlayer()
    {
        for (int i = 0; i < volleyShots; i++)
        {
            AcquireClosestPlayerIfNeeded();
            if (player == null) yield break;

            // Stop volley if we go offscreen (fairness)
            if (onlyFireOnScreen && !IsRendererOnGameCamera())
                yield break;

            Vector2 origin = firePoint.position;
            Vector2 target = GetPlayerCenterMass();
            Vector2 dir = (target - origin);

            if (dir.sqrMagnitude < 0.0001f)
                dir = sr.flipX ? Vector2.right : Vector2.left;

            dir.Normalize();

            float randomAngle = Random.Range(-volleyInaccuracyDegrees, volleyInaccuracyDegrees);
            dir = Rotate(dir, randomAngle);

            if (drawAim) Debug.DrawLine(origin, origin + dir * 2f, Color.cyan, 0.25f);

            BlasterBullet2D b = Instantiate(bulletPrefab, origin, Quaternion.identity);
            b.Fire(dir, volleySpeed);

            yield return new WaitForSeconds(volleyShotInterval);
        }
    }

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
        if (anim != null)
            anim.SetBool("isOpen", open);
    }

    private void ApplyWallSideFacing()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        bool faceRight = (wallSide == WallSide.LeftWall);
        sr.flipX = faceRight;

        if (firePoint != null)
        {
            Vector3 lp = firePoint.localPosition;
            lp.x = Mathf.Abs(lp.x) * (faceRight ? 1f : -1f);
            firePoint.localPosition = lp;
        }
    }

    // ----- Game camera visibility (ignores Scene view camera) -----

    private bool IsRendererOnGameCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return true;

        if (sr == null) return IsOnGameCameraPivotOnly();

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        return GeometryUtility.TestPlanesAABB(planes, sr.bounds);
    }

    private bool IsOnGameCameraPivotOnly()
    {
        Camera cam = Camera.main;
        if (cam == null) return true;

        Vector3 vp = cam.WorldToViewportPoint(transform.position);
        return (vp.z > 0f && vp.x > 0f && vp.x < 1f && vp.y > 0f && vp.y < 1f);
    }
}