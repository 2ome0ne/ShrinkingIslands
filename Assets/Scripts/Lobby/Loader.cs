using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
public static class Loader
{
    public enum Scene
    {
        GameScene,
        ReadyScene,
        Lobby,
        ChooseModifier,
        LoadingScene,
        WinScreen,
        MainMenu
    }

    private static Scene targetScene;

    public static void Load(Scene targetScene)
    {
        Loader.targetScene = targetScene;
        
        //SceneManager.LoadScene(Scene.LoadingScene.);
    }

    public static void DisconnectCallBack(Scene targetScene)
    {
        Loader.targetScene = targetScene;
        SceneManager.LoadScene(targetScene.ToString());
    }

    public static void LoadNetwork(Scene targetScene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }
}
