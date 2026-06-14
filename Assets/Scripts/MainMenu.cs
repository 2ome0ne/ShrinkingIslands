using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    
    [SerializeField] private FadeCamera _fadeCamera;
    private void Awake()
    {
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
    }

    public void GoLobby()
    {
        _fadeCamera.FadeOut();
        Invoke("GoLobbyWait", 1f);
    }
    
    public void GoLobbyWait()
    {
        SceneManager.LoadScene(Loader.Scene.Lobby.ToString());
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
