using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public void GoLobby()
    {
        SceneManager.LoadScene(Loader.Scene.Lobby.ToString());
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
