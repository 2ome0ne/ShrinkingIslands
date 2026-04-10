using System;
using System.Collections.Generic;
using Unity.Netcode;
using System.Collections;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
    //0 , 0 will be the spawn and will spawn two

    [SerializeField] private float RotaionPerPlayer;

    [SerializeField] private GameObject SpawnPodiumPrefab;

    [SerializeField] private GameObject SpawnCheckPrefab;

    [SerializeField] private int amount_off_players;

    [SerializeField] private float MoveAmount;

    private GameObject spawnCheck;

    [SerializeField] private List<Vector3> SpawnPoints;
    [SerializeField] private LayerMask GroundLayer;

    [SerializeField] private float CurrentRotaion;

    public Transform spawnpoint;

    public static SpawnManager Instance;

    [SerializeField] private GameManager gameManager;
    [SerializeField] private ReadyUp readyUp;

    public bool GenerateComplete;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            readyUp = FindFirstObjectByType<ReadyUp>();
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted;
        }
    }
    

    private void Awake()
    {
        Instance = this;
    }
    
    
    private void SceneManager_OnLoadEventCompleted(string sceneName, 
        UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, 
        System.Collections.Generic.List<ulong> clientsCompleted, 
        System.Collections.Generic.List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Debug.Log("startCal complete");
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
        Transform playerTransform = Instantiate(gameManager.playerPrefab);
        playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId , true);
        PlayerData playerData = readyUp.GetPlayerDataFromClientId(clientId);
        playerTransform.GetComponent<ThePlayerData>().PlayerName = playerData.name.ToString();
        playerTransform.GetComponent<ThePlayerData>().PlayerId.Value = clientId;
        playerTransform.GetComponent<ThePlayerData>().SetPlayerNameServerRpc(playerData.name.ToString());
        GetComponent<GameManager>().AddPlayerRpc(playerTransform.gameObject.GetComponent<NetworkObject>() , true);
        Vector3 spawnPos = CalulateSpawnPoint() + Vector3.up * 2f;
        SetPositionClientRpc(playerTransform.GetComponent<NetworkObject>() , spawnPos);
    }

    [ClientRpc]
    void SetPositionClientRpc(NetworkObjectReference objRef , Vector3 spawnPosition)
    {
        objRef.TryGet(out NetworkObject networkObject);
        networkObject.GetComponent<CharacterController>().enabled = false;
        networkObject.transform.position = spawnPosition;
        networkObject.GetComponent<CharacterController>().enabled = true;
    }
    
    public Vector3 CalulateSpawnPoint()
    {
        Debug.Log("CalulateSpawnPointServerRpc");
        Physics.SyncTransforms();
        CurrentRotaion += RotaionPerPlayer;
        Quaternion spawnCheckOriatation = Quaternion.Euler(0, CurrentRotaion, 0);
    
        // Create the checker
        GameObject checker = Instantiate(SpawnCheckPrefab, transform.position, spawnCheckOriatation);
        Transform checkTrans = checker.transform;
        checkTrans.position += checkTrans.forward * MoveAmount;

        bool hasDetectedLand = false;
        int maxIterations = 500; // Safety cap to prevent freezing
        int currentIteration = 0;
        Vector3 foundPos = Vector3.zero;
        while (!hasDetectedLand && currentIteration < maxIterations)
        {
            // Raycast from high up to ensure we hit the terrain
            hasDetectedLand = Physics.Raycast(checkTrans.position, Vector3.down, out RaycastHit hit, 100f, GroundLayer);
        
            if (!hasDetectedLand)
            {
                checkTrans.position -= checkTrans.forward * 0.5f; // Move back in larger chunks for performance
                currentIteration++;
            }
            else
            {
                Debug.Log("HIt : " + hit.collider.gameObject.name);
                // Successfully found land!
                GameObject podium = Instantiate(SpawnPodiumPrefab, hit.point, Quaternion.identity);
                foundPos = podium.transform.position; // Set the reference
                podium.GetComponent<NetworkObject>().Spawn(true);
            }
        }

        if (currentIteration >= maxIterations) Debug.LogWarning("SpawnManager: Could not find land!");
        return foundPos;
    }
}
