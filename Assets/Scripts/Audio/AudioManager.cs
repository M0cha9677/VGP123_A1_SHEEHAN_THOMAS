using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public static AudioManager Instance;

    [Header("Sounds")]
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip enemyDeathSound;
    [SerializeField] private AudioClip pickupSound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlayShoot()
    {
        sfxSource.PlayOneShot(shootSound);
    }

    public void PlayEnemyDeath()
    {
        sfxSource.PlayOneShot(enemyDeathSound);
    }

    public void PlayPickup()
    {
        sfxSource.PlayOneShot(pickupSound);
    }
}
