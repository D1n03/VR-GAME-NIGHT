using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the player presses the 'Q' key to quit
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
    }

    private void QuitGame()
    {
        // For testing in Editor
        Debug.Log("Quitting the game...");

        // Quit the game (works in built versions, but not in Editor)
        Application.Quit();

        // Stop play mode if in editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
