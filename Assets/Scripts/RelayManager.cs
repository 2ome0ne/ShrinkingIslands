using TMPro;//
using Unity.Services.Authentication;//
using Unity.Services.Core;//
using UnityEngine;
using System.Threading.Tasks;//
using Unity.Netcode;//
using Unity.Netcode.Transports.UTP;//
using Unity.Networking.Transport.Relay;//
using Unity.Services.Relay;//
using Unity.Services.Relay.Models;//
using System.Collections.Generic;
using System.Collections;
using Unity.Services.Lobbies.Models;

public class RelayManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI joinCodeText;
    [SerializeField] private TMP_InputField joinCodeInputField;
    [SerializeField] private TMP_InputField nameInputField;

    public List<Player> Players = new List<Player>();
    public string player_Name;
    public int player_color_index = -1;
    
    public int amountOfPlayers;
    
    private async void Awake()
    {
        DontDestroyOnLoad(gameObject);
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("SignedIn" + AuthenticationService.Instance.PlayerName);
        };
        if(!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void JoinRelay()
    {
        await StartClientWithRelay(joinCodeInputField.text);
    }

    public async Task<string> StartHostWithRelay(int maxConnections = 4)
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
            allocation.ToRelayServerData("dtls")
        );
        
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        
        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }


    public async Task<bool> StartClientWithRelay(string joinCode)
    {
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(joinAllocation.ToRelayServerData("dtls"));
        
        return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
    }
}
