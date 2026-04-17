using System;
using Unity.Netcode;
using UnityEngine;
using TMPro;
public class ThePlayerData : NetworkBehaviour
{
    public NetworkVariable<ulong> PlayerId = new NetworkVariable<ulong>();
    public TextMeshProUGUI PlayerNameText;
    [SerializeField] private Transform NameCanvas;
    public string PlayerName;
    
    [SerializeField] private PickUpSystem pickUpSystem;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientDisconnectCallback;
        }

        if (IsOwner)
        {
            PlayerNameText.gameObject.SetActive(false);
        }
    }

    [Rpc(SendTo.Everyone)]
    public void SetPlayerNameServerRpc(string playerName)
    {
        PlayerNameText.text = playerName;
    }

    public void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log("OnClientDisconnectCallback");
        if (clientId == OwnerClientId && pickUpSystem.CurrentHoldObject != null)
        {
            pickUpSystem.Destory_Item_Thats_CurrentlyHoldingServerRpc();
            GameManager.Instance.PlayerHasDisconnected(clientId);
        }
    }
}
