using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Load a specific scene by index (manual use if needed)
    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    // Call this from the "Next" button
    public void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int totalScenes = SceneManager.sceneCountInBuildSettings;

        if (currentIndex < totalScenes - 1)
        {
            SceneManager.LoadScene(currentIndex + 1);
        }
        else
        {
            QuitApp();
        }
    }

    // Quit application (Editor + Build safe)
    public void QuitApp()
    {
        Debug.Log("Quitting application");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
