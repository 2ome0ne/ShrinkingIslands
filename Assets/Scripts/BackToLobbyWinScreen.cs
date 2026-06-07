using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class BackToLobbyWinScreen : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;

    public override void OnNetworkSpawn()
    {
        GetPlayerName();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void GetPlayerName()
    {
        playerNameText.text = FindFirstObjectByType<PlayerWinsManager>().WinnerName;
    }
    public void BackToLobby()
    {
        NetworkManager.Singleton.Shutdown();
        var Readyups = FindFirstObjectByType<ReadyUp>();
        if(Readyups != null) Destroy(Readyups.GameObject());
        var modifierHolder = FindFirstObjectByType<ModifierHolder>();
        if(modifierHolder != null) Destroy(modifierHolder.GameObject());
        Destroy(NetworkManager.Singleton.GameObject());
        SceneManager.LoadScene(Loader.Scene.Lobby.ToString());
        
        RelayManager relay = FindFirstObjectByType<RelayManager>();
        Destroy(relay.gameObject);
        //Destroy(NetworkManager.Singleton.gameObject);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    [ClientRpc]
    private void DisconenctAllConnectedClientRpc()
    {
        if (IsHost) return;
        Debug.Log("Didnt Return >:(");
        var modifierHolder = FindFirstObjectByType<ModifierHolder>();
        if(modifierHolder != null) modifierHolder.gameObject.GetComponent<NetworkObject>().Despawn(true);
        NetworkManager.Singleton.Shutdown();
        var Readyups = FindFirstObjectByType<ReadyUp>();
        if(Readyups != null) Destroy(Readyups.GameObject());
        Destroy(NetworkManager.Singleton.GameObject());
        SceneManager.LoadScene(Loader.Scene.Lobby.ToString());
    }

    public void LeaveGame()
    {
        NetworkManager.Singleton.Shutdown();
        Destroy(NetworkManager.Singleton.GameObject());
        var Readyups = FindFirstObjectByType<ReadyUp>();
        if(Readyups != null) Destroy(Readyups.GameObject());
        SceneManager.LoadScene(Loader.Scene.Lobby.ToString());
    }
}
