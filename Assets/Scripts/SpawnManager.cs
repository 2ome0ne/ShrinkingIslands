using System;
using System.Collections.Generic;
using Unity.Netcode;
using System.Collections;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnManager : NetworkBehaviour
{
    //0 , 0 will be the spawn and will spawn two
    [SerializeField] private bool IsTestMode = false;

    [SerializeField] private float SpawnRange;

    [SerializeField] private GameObject SpawnPodiumPrefab;

    [SerializeField] private int amount_off_players;

    [SerializeField] private IslandHeart islandHeart;
    [SerializeField] private float MoveAmount;

    [SerializeField] private TheSea sea;

    public Transform spawnpoint;

    public static SpawnManager Instance;

    [SerializeField] private GameManager gameManager;
    [SerializeField] private ReadyUp readyUp;

    private int playerIndex = 0;

    public bool GenerateComplete;

    public override void OnNetworkSpawn()
    {
        if (IsTestMode) return;
        if (IsServer)
        {
            readyUp = FindFirstObjectByType<ReadyUp>();
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    public override void OnDestroy()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted;
        }
    }
    

    private void Awake()
    {
        Instance = this;
        amount_off_players = FindFirstObjectByType<RelayManager>().amountOfPlayers;
    }
    
    
    private void SceneManager_OnLoadEventCompleted(string sceneName, 
        UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, 
        System.Collections.Generic.List<ulong> clientsCompleted, 
        System.Collections.Generic.List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Debug.Log("startCal complete And Connected Clients =" + NetworkManager.Singleton.ConnectedClientsIds.Count);
            StartCoroutine(WaitUntilTerrainGenerates(clientId));
        }
    }

    IEnumerator WaitUntilTerrainGenerates(ulong clientId)
    {
        yield return new WaitUntil(() => GenerateComplete == true);
        Debug.Log("Sending SpawnPlayer");
        spawnPlayerServerRpc(clientId);
    }

    [ServerRpc]
    void spawnPlayerServerRpc(ulong clientId)
    {
        //Spawn Island Podium
        Vector3 spawnPos = GetSpawnIsland_SpawnPoint();
        GameObject SPS = Instantiate(SpawnPodiumPrefab, spawnPos + new Vector3( 0 , islandHeart.IslandSpawnY , 0), Quaternion.LookRotation(Vector3.zero - spawnPos));
        Vector3 PlayerSpawnPos = SPS.GetComponent<SpawnPodium>().PlayerSpawnPoint.position;
        SPS.GetComponent<NetworkObject>().Spawn();
        
        Transform playerTransform = Instantiate(gameManager.playerPrefab);
        playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId , true);
        PlayerData playerData = readyUp.GetPlayerDataFromClientId(clientId);
        var _playerData = playerTransform.GetComponent<ThePlayerData>();
        _playerData.SetIndexColorToAllRpc(playerData.IndexColor);
        playerTransform.GetComponent<PlayerPose>().SetColorRpc();
        _playerData.PlayerName = playerData.name.ToString();
        _playerData.PlayerId.Value = clientId;
        _playerData.SetPlayerNameServerRpc(playerData.name.ToString());
        GetComponent<GameManager>().AddPlayerRpc(playerTransform.gameObject.GetComponent<NetworkObject>() , true);
        sea.players.Add(playerTransform.gameObject);
        SetPositionClientRpc(playerTransform.GetComponent<NetworkObject>() , PlayerSpawnPos);
    }

    [ClientRpc]
    void SetPositionClientRpc(NetworkObjectReference objRef , Vector3 spawnPosition)
    {
        objRef.TryGet(out NetworkObject networkObject);
        networkObject.GetComponent<CharacterController>().enabled = false;
        networkObject.transform.position = spawnPosition;
        networkObject.GetComponent<CharacterController>().enabled = true;
    }

    public Vector3 GetSpawnIsland_SpawnPoint()
    {
        Debug.Log("Player Number :" + amount_off_players + " Player Index : " + playerIndex);
        float angle = (360f / amount_off_players) * playerIndex * Mathf.Deg2Rad;

        float x = Mathf.Cos(angle) * SpawnRange;
        float z = Mathf.Sin(angle) * SpawnRange;
    
        playerIndex++;
        return new Vector3(x, 0f, z);
    }
}
