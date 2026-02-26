using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneToggleManager : MonoBehaviour
{
    [Header("Scene Names (must match Build Settings)")]
    [SerializeField] private string titleSceneName = "Title";
    [SerializeField] private string gameSceneName = "GutsManLevel";

    private static SceneToggleManager _instance;

    private void Awake()
    {
        // Singleton so you don't get duplicates when switching scenes
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        string current = SceneManager.GetActiveScene().name;

        if (current == titleSceneName)
            SceneManager.LoadScene(gameSceneName);
        else
            SceneManager.LoadScene(titleSceneName);
    }
}
