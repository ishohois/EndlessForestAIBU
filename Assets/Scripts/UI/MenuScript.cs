using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        Debug.Log("Starting...");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting...");
    }

    public static void LoadScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }
}
