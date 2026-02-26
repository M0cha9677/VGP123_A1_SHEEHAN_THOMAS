using System.Collections;
using UnityEngine;

public class PlayerStats2D : MonoBehaviour
{
    [Header("Health / Lives")]
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private int startLives = 3;

    [Header("Shooting Energy")]
    [SerializeField] private int maxEnergy = 20;
    [SerializeField] private int startEnergy = 0;     // you said you want 0
    [SerializeField] private int energyPerShot = 1;

    [Header("Hurt / I-Frames")]
    [SerializeField] private float iFrameTime = 0.8f;
    [SerializeField] private float hurtLockTime = 0.15f;

    [Header("Respawn")]
    [Tooltip("Optional. If null, will auto-find GameObject with tag 'Respawn'.")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float respawnInvulnTime = 1.0f;
    [SerializeField] private bool refillEnergyOnRespawn = true;

    [Header("Game Over UI (optional)")]
    [SerializeField] private GameObject gameOverPanel;

    private int _health;
    private int _lives;
    private int _energy;

    private bool _invulnerable;
    private bool _dead;

    private Animator _anim;
    private SpriteRenderer _sr;
    private Rigidbody2D _rb;
    private PlayerMovement2D _move;
    private Collider2D _col;

    public int Health => _health;
    public int Lives => _lives;
    public int Energy => _energy;
    public bool IsDead => _dead;

    public bool CanShoot => !_dead && _energy >= energyPerShot;

    private void Awake()
    {
        _health = maxHealth;
        _lives = startLives;
        _energy = Mathf.Clamp(startEnergy, 0, maxEnergy);

        _anim = GetComponent<Animator>();
        _sr = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _move = GetComponent<PlayerMovement2D>();
        _col = GetComponent<Collider2D>();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void Start()
    {
        if (respawnPoint == null)
        {
            GameObject rp = GameObject.FindGameObjectWithTag("Respawn");
            if (rp != null) respawnPoint = rp.transform;
        }
    }

    // ---------------- ENERGY ----------------
    public void AddEnergy(int amount)
    {
        if (_dead) return;
        _energy = Mathf.Clamp(_energy + amount, 0, maxEnergy);
    }

    public bool TrySpendEnergyForShot()
    {
        if (!CanShoot) return false;
        _energy -= energyPerShot;
        return true;
    }

    // ---------------- HEALTH ----------------
    public void Heal(int amount)
    {
        if (_dead) return;
        _health = Mathf.Clamp(_health + amount, 0, maxHealth);
    }

    public void AddLife(int amount = 1) => _lives += amount;

    public void TakeDamage(int amount)
    {
        if (_dead || _invulnerable) return;
        if (amount <= 0) return;

        _health -= amount;

        if (_health <= 0)
        {
            BeginDeath();
            return;
        }

        if (_anim != null) _anim.SetTrigger("hurt");
        StartCoroutine(HurtRoutine());
    }

    private IEnumerator HurtRoutine()
    {
        _invulnerable = true;

        if (_move != null) _move.enabled = false;
        yield return new WaitForSeconds(hurtLockTime);
        if (_move != null && !_dead) _move.enabled = true;

        // blink i-frames
        float timer = 0f;
        while (timer < iFrameTime)
        {
            timer += 0.1f;
            if (_sr != null) _sr.enabled = !_sr.enabled;
            yield return new WaitForSeconds(0.1f);
        }

        if (_sr != null) _sr.enabled = true;
        _invulnerable = false;
    }

    // ---------------- DEATH / RESPAWN ----------------

    private void BeginDeath()
    {
        if (_dead) return;

        _dead = true;
        _lives--;

        // Lock player immediately
        if (_move != null) _move.enabled = false;
        if (_col != null) _col.enabled = false; // prevents hazards/bullets during death
        if (_rb != null) _rb.linearVelocity = Vector2.zero;

        if (_anim != null)
        {
            _anim.SetBool("isDead", true);
            _anim.SetTrigger("die");
        }

        // IMPORTANT:
        // Respawn is triggered by an Animation Event calling OnDeathAnimFinished()
        // (see instructions below)
    }

    // CALL THIS FROM AN ANIMATION EVENT on the LAST frame of the DIE clip.
    public void OnDeathAnimFinished()
    {
        if (_lives > 0)
        {
            StartCoroutine(RespawnRoutine());
        }
        else
        {
            GameOver();
        }
    }

    private IEnumerator RespawnRoutine()
    {
        // Reset stats
        _health = maxHealth;
        if (refillEnergyOnRespawn)
            _energy = Mathf.Clamp(startEnergy, 0, maxEnergy); 

        // Move to spawn
        if (respawnPoint != null)
            transform.position = respawnPoint.position;

        // Reset physics
        if (_rb != null) _rb.linearVelocity = Vector2.zero;

        // Re-enable
        _dead = false;
        if (_anim != null) _anim.SetBool("isDead", false);

        if (_col != null) _col.enabled = true;
        if (_move != null) _move.enabled = true;

        // Short invulnerability after respawn
        _invulnerable = true;
        float t = 0f;
        while (t < respawnInvulnTime)
        {
            t += 0.1f;
            if (_sr != null) _sr.enabled = !_sr.enabled;
            yield return new WaitForSeconds(0.1f);
        }
        if (_sr != null) _sr.enabled = true;
        _invulnerable = false;
    }

    private void GameOver()
    {
        // Keep player locked
        if (_move != null) _move.enabled = false;
        if (_col != null) _col.enabled = false;
        if (_rb != null) _rb.linearVelocity = Vector2.zero;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Debug.Log("GAME OVER");
    }
}