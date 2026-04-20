using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
    }

    public void GoLobby()
    {
        SceneManager.LoadScene(Loader.Scene.Lobby.ToString());
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
