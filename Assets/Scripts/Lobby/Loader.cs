using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
public static class Loader
{
    public enum Scene
    {
        SampleScene,
        ReadyScene,
        Lobby
    }

    private static Scene targetScene;

    public static void Load(Scene targetScene)
    {
        Loader.targetScene = targetScene;
        
        //SceneManager.LoadScene(Scene.LoadingScene.);
    }

    public static void LoadNetwork(Scene targetScene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }
}
