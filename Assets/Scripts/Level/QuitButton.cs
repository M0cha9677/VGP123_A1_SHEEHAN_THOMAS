using UnityEngine;

public class QuitButton : MonoBehaviour
{

    protected void QuitGame()
    {

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif

    }
}