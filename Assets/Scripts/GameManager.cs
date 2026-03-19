using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public Transform playerPrefab;
    [SerializeField] private SpawnManager spawnManager;
    public TerrainGeneration terrainGenerator;
    public static GameManager Instance { get; private set; }
    public int AmountOfHold;
    public List<ActivePlayer> Players;

    [SerializeField] private GameObject SpectatorPlayer;

    [System.Serializable]
    public class ActivePlayer
    {
        public Transform player;
        public ulong playerId;
        public bool isAlive = true;
        public bool isWinner = false;
    }
    
    public void AddPlayer(Transform player, bool isAlive)
    {
        ActivePlayer activePlayer = new ActivePlayer();
        activePlayer.player = player;
        activePlayer.isAlive = isAlive;
        activePlayer.playerId = player.GetComponent<ThePlayerData>().PlayerId.Value;
        Players.Add(activePlayer);
    }
    [ServerRpc]
    public void PlayerDiesServerRpc(Vector3 playerDeathPosition , ulong playerId , NetworkObjectReference playerRef)
    {
        GameObject spectatorPlayer = Instantiate(SpectatorPlayer.gameObject, playerDeathPosition, Quaternion.identity);
        spectatorPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(playerId, true);
        playerRef.TryGet(out NetworkObject player);
        player.Despawn();
        ActivePlayer deadplayer = Players.Find(player => player.playerId == playerId);
        deadplayer.isAlive = false;
        CheckForWinnerServerRpc();
    }

    [ServerRpc]
    private void CheckForWinnerServerRpc()
    {
        int amountOfAlivePlayers = 0;
        Transform player = new RectTransform();
        foreach (ActivePlayer activePlayer in Players)
        {
            if (activePlayer.isAlive)
            {
                amountOfAlivePlayers++;
                player = activePlayer.player;
            }
        }

        if (amountOfAlivePlayers == 1)
        {
            //WE HAVE A WINNER
            Debug.Log("Game Over" + player.GetComponent<ThePlayerData>().PlayerId.Value + " Won");
            ulong winnerPlayerId = player.GetComponent<ThePlayerData>().PlayerId.Value;
            ActivePlayer winnerPlayer = Players.Find(player => player.playerId == winnerPlayerId);
            winnerPlayer.isWinner = true;
        }
        else
        {
            Debug.Log("no one won yet");
        }
    }

    [ServerRpc]
    public void StartLoadCompleteServerRpc()
    {
       // NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
    }

    public void PlayerHasDisconnected(ulong ClientId)
    {
        ActivePlayer player = Players.Find(player => player.playerId == ClientId);
        Players.Remove(player);
    }

    private void Awake()
    {
        Instance = this;
        if (Instance != null && Instance != this && !IsServer)
        {
            NetworkObject.Despawn();
        }
    }


}
