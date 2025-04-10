using UnityEngine;

public class GameController : MonoBehaviour
{
    void Update()
    {
        // Check if the ESC key was pressed this frame
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
    }

    void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        #if UNITY_EDITOR
            // If we are running in the editor
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // If we are running in a build
            Application.Quit();
        #endif
    }
} 