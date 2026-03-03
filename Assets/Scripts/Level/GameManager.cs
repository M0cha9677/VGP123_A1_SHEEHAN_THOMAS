using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameState { Title, Playing, GameOver }

    public static GameManager Instance { get; private set; }
    public GameState State { get; private set; } = GameState.Title;

    [Header("Scenes (must match Build Settings names)")]
    [SerializeField] private string titleSceneName = "Title";
    [SerializeField] private string gameSceneName = "GutsManLevel";

    [Header("Start Rules")]
    [SerializeField] private int startingLives = 3;
    [SerializeField] private bool startGameFromTitleOnEsc = true;

    [Header("UI (scene object names)")]
    [Tooltip("Exact hierarchy name of the panel in the GAME scene.")]
    [SerializeField] private string gameOverPanelName = "gameOverPanel";

    private GameObject _gameOverPanel;
    private PlayerStats2D _player;
    private bool _waitingForPlayerReset;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;

        if (scene.name == titleSceneName)
        {
            State = GameState.Title;
            _gameOverPanel = null;

            // Optional: clear player ref when leaving gameplay
            _player = null;
            _waitingForPlayerReset = false;
            return;
        }

        if (scene.name == gameSceneName)
        {
            State = GameState.Playing;

            // Grab & hide GameOver panel in the GAME scene
            _gameOverPanel = GameObject.Find(gameOverPanelName);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);

            // Player may spawn later -> wait for registration
            _waitingForPlayerReset = true;

            // If player already exists (e.g., placed in scene), reset immediately
            if (_player != null)
            {
                ResetRegisteredPlayer();
            }

            return;
        }
    }

    private void Update()
    {
        if (State == GameState.Title && startGameFromTitleOnEsc)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                StartGame();
        }
        else if (State == GameState.GameOver)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                GoToTitle();
        }
    }

    // ---------------- PUBLIC API ----------------

    public void StartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void GoToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(titleSceneName);
    }

    public void TriggerGameOver()
    {
        State = GameState.GameOver;

        if (_gameOverPanel == null)
            _gameOverPanel = GameObject.Find(gameOverPanelName);

        if (_gameOverPanel != null)
            _gameOverPanel.SetActive(true);
        else
            Debug.LogWarning($"GameOver panel '{gameOverPanelName}' not found in scene.");
    }

    // (Optional) Call this from LevelEndTrigger if you want to go back to title
    public void LevelCompleteToTitle()
    {
        GoToTitle();
    }

    // ---------------- INTERNAL ----------------

    private void ResetPlayerForNewRun()
    {
        PlayerStats2D stats = FindFirstObjectByType<PlayerStats2D>();
        if (stats == null)
        {
            Debug.LogWarning("No PlayerStats2D found to reset.");
            return;
        }

        stats.ResetForNewGame(startingLives);
        stats.ForceRespawnNow();
    }

    public void RegisterPlayer(PlayerStats2D stats)
    {
        _player = stats;
    }

    private void ResetRegisteredPlayer()
    {
        if (_player == null) return;

        _player.ResetForNewGame(startingLives);
        _player.ForceRespawnNow();

        _waitingForPlayerReset = false;
    }
}
