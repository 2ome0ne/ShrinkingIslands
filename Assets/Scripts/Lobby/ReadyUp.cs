using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using Unity.Netcode;
using Unity.Networking.Transport;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class ReadyUp : NetworkBehaviour
{
    private Dictionary<ulong, bool> playerReadyDic;
    [SerializeField] private RelayManager relayManager;
    private NetworkList<PlayerData> playerDataNetworkList;
    public Loader.Scene ChangeScene = Loader.Scene.GameScene;
    public bool AllowChange = false;
    private void Awake()
    {
        playerReadyDic = new Dictionary<ulong, bool>();
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_ConnectedCallBack;
        relayManager = FindAnyObjectByType<RelayManager>();
        playerDataNetworkList = new NetworkList<PlayerData>();
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= NetworkManager_Client_ConnectedCallBack;
    }

    [Rpc(SendTo.Everyone)]
    public void ChangeScenesRpc(Loader.Scene scene)
    {
        ChangeScene = scene;
        AllowChange = true;
        Loader.LoadNetwork(Loader.Scene.LoadingScene);
    }

    private void OnEnable()
    {
// Subscribe to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
// Unsubscribe to avoid memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!AllowChange) return;
        SetPlayerReady();
        Debug.Log("ReadyUp Initialized");
        //DontDestroyOnLoad(this.gameObject);
        AllowChange = false;
    }
    
    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if (playerData.clientId == clientId)
            {
                return playerData;
            }
        }
        return default;
    }

    public int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
            {
                return i;
            }
        }

        return -1;
    }

    [ServerRpc]
    private void SetPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        
        playerData.name = playerName;
        
        playerDataNetworkList[playerDataIndex] = playerData;
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("ReadyUp spawned 0");
        HostNameSetterServerRpc();
        SetPlayerReady();
        Debug.Log("ReadyUp spawned");
        DontDestroyOnLoad(this.gameObject);
    }

    [ServerRpc]
    private void HostNameSetterServerRpc()
    {

    }
    
    private void NetworkManager_Client_ConnectedCallBack(ulong ClientId)
    {
        Debug.Log(ClientId.ToString() + "Connected to Server");
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void TestNetworkSetPlayerNameServerRpc(ulong clientId, string playerName)
    {
        playerDataNetworkList.Add(new PlayerData {clientId = clientId, name = playerName});
    }
    
    public void SetPlayerReady()
    {
        //if(!IsOwner) return;
        SetPlayer_ReadyServerRpc( ChangeScene , relayManager.player_Name);
    }
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetPlayer_ReadyServerRpc(Loader.Scene loadScene ,FixedString64Bytes playerName , RpcParams serverRpcPrams = default)
    {
        Debug.Log($"Client {serverRpcPrams.Receive.SenderClientId} readied up!" + " Name Is :" + playerName);
        TestNetworkSetPlayerNameServerRpc(serverRpcPrams.Receive.SenderClientId , playerName.ToString());
        playerReadyDic[serverRpcPrams.Receive.SenderClientId] = true;
        
        bool allClientsReady = true;
        foreach (ulong cliendId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDic.ContainsKey(cliendId) || !playerReadyDic[cliendId])
            {
                allClientsReady = false;
                break;
            }
        }

        if (NetworkManager.Singleton.ConnectedClientsIds.Count != relayManager.amountOfPlayers)
        {
            allClientsReady = false;
            Debug.Log("Not ENOUGH");
            StartCoroutine(WaitForAllClientsToConnect(playerName.ToString() , loadScene));
        }

        if (allClientsReady)
        {
            Loader.LoadNetwork(loadScene);
        }
        //doesnt work \/ ;p
        if (NetworkManager.Singleton.ConnectedClientsIds.Count == 1)
        {
            Debug.Log("no firends ;(");
            NetworkManager.Singleton.Shutdown();
            var Readyups = FindFirstObjectByType<ReadyUp>();
            if(Readyups != null) Destroy(Readyups.GameObject());
            var modifierHolder = FindFirstObjectByType<ModifierHolder>();
            if(modifierHolder != null) Destroy(modifierHolder.GameObject());
            Destroy(NetworkManager.Singleton.GameObject());
            SceneManager.LoadScene(Loader.Scene.Lobby.ToString());
            ErrorMessageManager.instance.ShowError("you got no friends? ;-)");
        }
    }

    IEnumerator WaitForAllClientsToConnect(string playerName , Loader.Scene scene)
    {
        Debug.Log("they allowed this some how");
        yield return new WaitUntil(()=> NetworkManager.Singleton.ConnectedClientsIds.Count == relayManager.amountOfPlayers);
        SetPlayer_ReadyServerRpc(scene , playerName);
    }
    
    
    //OLD
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetPlayerReadyServerRpc(FixedString64Bytes playerName , RpcParams serverRpcPrams = default)
    {
        Debug.Log($"Client {serverRpcPrams.Receive.SenderClientId} readied up!" + " Name Is :" + playerName);
        TestNetworkSetPlayerNameServerRpc(serverRpcPrams.Receive.SenderClientId , playerName.ToString());
        playerReadyDic[serverRpcPrams.Receive.SenderClientId] = true;
        
        bool allClientsReady = true;
        foreach (ulong cliendId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDic.ContainsKey(cliendId) || !playerReadyDic[cliendId])
            {
                allClientsReady = false;
                break;
            }
        }

        if (NetworkManager.Singleton.ConnectedClientsIds.Count != relayManager.amountOfPlayers)
        {
            allClientsReady = false;
            Debug.Log("Not ENOUGH");
            //StartCoroutine(WaitForAllClientsToConnect(playerName.ToString() , loadScene));
        }

        if (allClientsReady)
        {
            Loader.LoadNetwork(Loader.Scene.GameScene);
        }

        if (relayManager.amountOfPlayers == 1)
        {
            Debug.Log("Loaded1");
            Loader.LoadNetwork(Loader.Scene.GameScene);
        }
    }
}
