using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Naviguation : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync("ZoneDev");
    }

    public void MainMenu()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
