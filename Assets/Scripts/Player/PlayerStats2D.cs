using System.Collections;
using UnityEngine;

public class PlayerStats2D : MonoBehaviour
{
    [Header("Health / Lives")]
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private int startLives = 3;

    [Header("Shooting Energy")]
    [SerializeField] private int maxEnergy = 20;
    [SerializeField] private int startEnergy = 0;   // set to 0 if player must collect energy first
    [SerializeField] private int energyPerShot = 1;

    [Header("Hurt / I-Frames")]
    [SerializeField] private float iFrameTime = 0.8f;
    [SerializeField] private float hurtLockTime = 0.15f;

    private int _health;
    private int _lives;
    private int _energy;

    private bool _invulnerable;
    private bool _dead;

    private Animator _anim;
    private PlayerMovement2D _move;
    private SpriteRenderer _sr;

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
        _move = GetComponent<PlayerMovement2D>();
    }

    // -----------------------
    // ENERGY
    // -----------------------

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

    // -----------------------
    // HEALTH
    // -----------------------

    public void Heal(int amount)
    {
        if (_dead) return;
        _health = Mathf.Clamp(_health + amount, 0, maxHealth);
    }

    public void AddLife(int amount = 1)
    {
        _lives += amount;
    }

    public void TakeDamage(int amount)
    {
        if (_dead || _invulnerable) return;
        if (amount <= 0) return;

        _health -= amount;

        if (_health <= 0)
        {
            Die();
            return;
        }

        if (_anim != null) _anim.SetTrigger("hurt");
        StartCoroutine(HurtRoutine());
    }

    private IEnumerator HurtRoutine()
    {
        _invulnerable = true;

        // Brief movement lock
        if (_move != null) _move.enabled = false;
        yield return new WaitForSeconds(hurtLockTime);
        if (_move != null && !_dead) _move.enabled = true;

        // Classic blink effect
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

    private void Die()
    {
        _dead = true;
        _lives--;

        if (_anim != null)
        {
            _anim.SetBool("isDead", true);
            _anim.SetTrigger("die");
        }

        if (_move != null)
            _move.enabled = false;

        Debug.Log($"Player died. Lives remaining: {_lives}");

        // For assignment simplicity:
        // You can reload scene or respawn later.
    }
}