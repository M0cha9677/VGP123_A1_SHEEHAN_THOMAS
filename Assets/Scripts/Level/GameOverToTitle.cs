using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverToTitle : MonoBehaviour
{
    [SerializeField] private string titleSceneName = "Title";

    private void OnEnable() => Time.timeScale = 1f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene(titleSceneName);
    }
}
