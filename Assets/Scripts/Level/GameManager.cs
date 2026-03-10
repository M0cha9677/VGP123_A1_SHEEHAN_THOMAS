using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameState { Title, Playing, Paused, GameOver }

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

    [Header("Pause UI (Scene object names)")]
    [SerializeField] private string pausePanel = "pausePanel";
    private GameObject _pausePanel;

    private GameObject _gameOverPanel;
    private PlayerStats2D _player;
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
            //_waitingForPlayerReset = false;
            return;
        }

        if (scene.name == gameSceneName)
        {
            State = GameState.Playing;

            // Grab & hide GameOver panel in the GAME scene
            _gameOverPanel = GameObject.Find(gameOverPanelName);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);

            // Player may spawn later -> wait for registration
            //_waitingForPlayerReset = true;

            // If player already exists (e.g., placed in scene), reset immediately
            if (_player != null)
            {
                ResetRegisteredPlayer();
            }

            _pausePanel = GameObject.Find(pausePanel);
            if (_pausePanel != null) _pausePanel.SetActive(false); 

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

        if (State == GameState.Playing || State == GameState.Paused)
        {
            if (Input.GetKeyDown(KeyCode.P))
                TogglePause();
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

        Time.timeScale = 0f;
    }

    // (Optional) Call this from LevelEndTrigger if you want to go back to title
    public void LevelCompleteToTitle()
    {
        GoToTitle();
    }

    // ---------------- INTERNAL ----------------

    public void RegisterPlayer(PlayerStats2D stats)
    {
        _player = stats;
    }

    private void ResetRegisteredPlayer()
    {
        if (_player == null) return;

        _player.ResetForNewGame(startingLives);
        _player.ForceRespawnNow();

        //_waitingForPlayerReset = false;
    }

    public void TogglePause()
    {
        if (State == GameState.Playing) Pause();
        else if (State == GameState.Paused) Resume();
    }

    public void Pause()
    {
        State = GameState.Paused;

        if (_pausePanel == null)
            _pausePanel = GameObject.Find(pausePanel);

        if (_pausePanel != null)
            _pausePanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void Resume()
    {
        State = GameState.Playing;

        if (_pausePanel != null)
            _pausePanel.SetActive(false);

        Time.timeScale = 1f;
    }
}
