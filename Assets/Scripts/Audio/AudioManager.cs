using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource uiSource;

    [Header("Music")]
    [SerializeField] private AudioClip titleMusic;
    [SerializeField] private AudioClip stageMusic;
    [SerializeField] private AudioClip gameOverMusic;

    [Header("UI / Menu SFX")]
    [SerializeField] private AudioClip menuMoveSfx;
    [SerializeField] private AudioClip menuConfirmSfx;
    [SerializeField] private AudioClip pauseSfx;

    [Header("Gameplay SFX")]
    [SerializeField] private AudioClip playerShootSfx;
    [SerializeField] private AudioClip playerDamagedSfx;
    [SerializeField] private AudioClip playerLandedSfx;
    [SerializeField] private AudioClip enemyShootSfx;
    [SerializeField] private AudioClip enemyDamagedSfx;
    [SerializeField] private AudioClip enemyDeathSfx;
    [SerializeField] private AudioClip pickupSfx;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ---------------- MUSIC ----------------

    public void PlayTitleMusic()
    {
        PlayMusic(titleMusic);
    }

    public void PlayStageMusic()
    {
        PlayMusic(stageMusic);
    }

    public void PlayGameOverMusic()
    {
        PlayMusic(gameOverMusic);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    // ---------------- UI SFX ----------------

    public void PlayMenuMove()
    {
        PlayUISfx(menuMoveSfx);
    }

    public void PlayMenuConfirm()
    {
        PlayUISfx(menuConfirmSfx);
    }

    public void PlayPause()
    {
        PlayUISfx(pauseSfx);
    }

    private void PlayUISfx(AudioClip clip)
    {
        if (uiSource == null || clip == null) return;
        uiSource.PlayOneShot(clip);
    }

    // ---------------- GAMEPLAY SFX ----------------

    public void PlayPlayerShoot()
    {
        PlaySfx(playerShootSfx);
    }

    public void PlayPlayerDamaged()
    {
        PlaySfx(playerDamagedSfx);
    }

    public void PlayPlayerLanded()
    {
        PlaySfx(playerLandedSfx);
    }

    public void PlayEnemyShoot()
    {
        PlaySfx(enemyShootSfx);
    }

    public void PlayEnemyDamaged()
    {
        PlaySfx(enemyDamagedSfx);
    }

    public void PlayEnemyDeath()
    {
        PlaySfx(enemyDeathSfx);
    }

    public void PlayPickup()
    {
        PlaySfx(pickupSfx);
    }

    private void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }
}
