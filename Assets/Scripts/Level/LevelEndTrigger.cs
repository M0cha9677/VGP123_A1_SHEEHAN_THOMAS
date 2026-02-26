using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndTrigger2D : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject levelCompletePanel;

    [Header("Scene Fallback (if no SceneToggleManager exists)")]
    [SerializeField] private string titleSceneName = "Title";

    [Header("Behavior")]
    [SerializeField] private bool freezeTime = true;

    private bool _ended;

    private void Start()
    {
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_ended) return;

        if (other.GetComponent<PlayerMovement2D>() == null) return;

        EndLevel();
    }

    private void EndLevel()
    {
        _ended = true;

        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(true);

        if (freezeTime)
            Time.timeScale = 0f;
    }

    private void Update()
    {
        if (!_ended) return;

        if (Input.GetKeyDown(KeyCode.Escape) ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.Space))
        {
            if (freezeTime)
                Time.timeScale = 1f;

            // Prefer your global manager (one source of truth)
            SceneToggleManager mgr = FindFirstObjectByType<SceneToggleManager>();
            if (mgr != null)
            {
                mgr.GoToTitle(); // you'll add this method (see below)
            }
            else
            {
                SceneManager.LoadScene(titleSceneName);
            }
        }
    }

    private void OnDisable()
    {
        if (freezeTime)
            Time.timeScale = 1f;
    }
}