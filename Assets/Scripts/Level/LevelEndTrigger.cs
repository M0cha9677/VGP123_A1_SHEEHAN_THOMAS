using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndTrigger2D : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject levelCompletePanel;

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

            if (GameManager.Instance != null)
                GameManager.Instance.LevelCompleteToTitle();
        }
           
    }

    private void OnDisable()
    {
        if (freezeTime)
            Time.timeScale = 1f;
    }
}