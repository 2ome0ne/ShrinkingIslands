using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode; //
using Unity.Services.Authentication;//
using Unity.Services.Core;//
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//using UnityEditor.VersionControl;

public class GameLobby : MonoBehaviour
{

    private Lobby HostLobby;
    private float heartbeatTimer;
    [SerializeField] private TMP_InputField CodeInputField;
    [SerializeField] private TextMeshProUGUI CodeText;
    [SerializeField] private TMP_InputField PlayerInputField;
    [SerializeField] private TextMeshProUGUI PlayerNameText;

    [SerializeField] private GameObject HostAndJoinButton;
    [SerializeField] private GameObject StartButton;
    [SerializeField] private GameObject StartDiscconectButton;
    [SerializeField] private GameObject nameShow;
    [SerializeField] private Transform Content;

    [SerializeField] private RelayManager relayManager;
    private const string PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER = "PlayerNameMultiplayer";
    private string playerName;
    private bool HasStarted = false;
    private string lobbyPassword;
    private Player playerData;
    private Lobby current_lobby;

    private void Update()
    {
        HadleLobbyHartbeat();
    }

    public void BackToMainMenu()
    {
        Destroy(NetworkManager.Singleton.GameObject());
        var relay = FindAnyObjectByType<RelayManager>();
        Destroy(relay.GameObject());
        SceneManager.LoadScene(Loader.Scene.MainMenu.ToString());
    }

    private async void HadleLobbyHartbeat()
    {
        if (HostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer <= 0)
            {
                float heartbeatTimerMax = 15;
                heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(HostLobby.Id);
            }
        }
    }

    public void CopyCodeButton()
    {
        GUIUtility.systemCopyBuffer = CodeText.text;
    }

    public async void CreateLobby()
    {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayers = 4;
            CreateProfile();
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = true,
                Player = playerData,
            };

            DataObject dataObjectJoinCode = new DataObject(DataObject.VisibilityOptions.Public, string.Empty);
            createLobbyOptions.Data = new Dictionary<string, DataObject> { { "JoinCode", dataObjectJoinCode } };
            
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers , createLobbyOptions);
            HostLobby = lobby;
            lobbyPassword = lobby.Id;
            Debug.Log(lobby.LobbyCode);
            CodeText.text = lobby.LobbyCode;
            StartDiscconectButton.SetActive(true);
            UpdateLobbyInfo();
            JoinedLobby();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinLobbyByCode()
    {
        try
        {
            string _lobbyCode = CodeInputField.text;
            CreateProfile();
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(_lobbyCode, new JoinLobbyByCodeOptions{ Player = playerData});
            lobbyPassword = lobby.Id;
            Debug.Log("Joined lobby: " + _lobbyCode);
            StartDiscconectButton.SetActive(true);
            UpdateLobbyInfo();
            JoinedLobby();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void CreateProfile()
    {
        playerName = PlayerInputField.text;
        PlayerDataObject playerDataObjectName = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName);

        playerData = new Player(id: AuthenticationService.Instance.PlayerId,
            data: new Dictionary<string, PlayerDataObject> {{ "Name", playerDataObjectName } , {"PlayerNumberID" , new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public)}});
    }
    
    public void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in Lobby" + lobby.Name);
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["Name"].Value);
        }
    }

    private void JoinedLobby()
    {
        HostAndJoinButton.SetActive(false);
        PlayerNameText.text = playerName;
    }
    
    public async void Disconnect()
    {
        try
        {
            lobbyPassword = null;
            await LobbyService.Instance.RemovePlayerAsync(current_lobby.Id, AuthenticationService.Instance.PlayerId);
            HostAndJoinButton.SetActive(true);
            StartDiscconectButton.SetActive(false);
            current_lobby = null;
            foreach (Transform t in Content)
            {
                Destroy(t.gameObject);
            }
            Debug.Log("Left lobby successfully");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public void LobbyStart()
    {
        HasStarted = true;
    }

    private bool isJoined = false;
    private string player_Name;
    public async void UpdateLobbyInfo()
    {
        while (Application.isPlaying)
        {
            if (string.IsNullOrEmpty(lobbyPassword))
            {
                return;
            }

            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(lobbyPassword);
            if (!isJoined && lobby.Data["JoinCode"].Value != string.Empty)
            {
                await relayManager.StartClientWithRelay(lobby.Data["JoinCode"].Value);
                
                Debug.Log("Joined :DDD");
                isJoined = true;
                return;
            }
            
            foreach (Transform t in Content)
            {
                Destroy(t.gameObject);
            }

            if (AuthenticationService.Instance.PlayerId == lobby.HostId)
            {
                StartButton.SetActive(true);
            }
            else
            {
                StartButton.SetActive(false);
            }
            relayManager.amountOfPlayers = lobby.Players.Count;
            relayManager.Players = lobby.Players;
            relayManager.player_Name = playerName;
            current_lobby = lobby;
            foreach (Player player in lobby.Players)
            {
                GameObject newPlayerItem = Instantiate(nameShow, Content);
                //Debug.Log(player.Data["Name"].Value + "EEEEEEEE");
                newPlayerItem.GetComponent<TextMeshProUGUI>().text = player.Data["Name"].Value;
            }
            
            if (HasStarted)
            {
                string JoinCode = await relayManager.StartHostWithRelay(lobby.MaxPlayers);
                isJoined = true;
                await LobbyService.Instance.UpdateLobbyAsync(lobbyPassword, new UpdateLobbyOptions{Data = new Dictionary<string, DataObject>{ {"JoinCode", new DataObject(DataObject.VisibilityOptions.Public, JoinCode)}}});
                Loader.LoadNetwork(Loader.Scene.ReadyScene);
            }
            
            await System.Threading.Tasks.Task.Delay(1000);
        }
    }
}
