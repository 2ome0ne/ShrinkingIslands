using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Collections;
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
    [SerializeField] private float OceanKnockBackForce = 40;
    [SerializeField] private GameObject WinnersUI;
    [SerializeField] private GameObject WinUI;
    [SerializeField] private GameObject LoseUI;
    public bool GameOver = false;
    public EscapeMenu escapeMenu;
    private bool disconnected = false;

    [SerializeField]
    private GameObject PlayerPointPrefab;

    [SerializeField] private Transform playerPointContent;

    [System.Serializable]
    public class ActivePlayer
    {
        public Transform player;
        public ulong playerId;
        public bool isAlive = true;
        public bool isWinner = false;
        public int currentWinPoints = 0;

        public bool IsImmune = false;

        //Current life max is 3
        public PlayerHealth playerHealth;
    }

    [Rpc(SendTo.Everyone)]
    public void AddPlayerRpc(NetworkObjectReference ObjRefrence, bool isAlive)
    {
        ObjRefrence.TryGet(out NetworkObject NetObj);
        Transform player;
        player = NetObj.transform;
        
        ActivePlayer activePlayer = new ActivePlayer();
        activePlayer.player = player;
        activePlayer.isAlive = isAlive;
        activePlayer.playerHealth = player.gameObject.GetComponent<PlayerHealth>();
        activePlayer.playerId = player.GetComponent<ThePlayerData>().PlayerId.Value;
        if (activePlayer.playerId == NetworkManager.LocalClientId)
        {
            escapeMenu.player = player.gameObject;
            escapeMenu.GetAllRefrences();
        }

        Players.Add(activePlayer);
    }

    public void SentEscapePlayer(GameObject player)
    {
        escapeMenu.player = player;
        escapeMenu.GetAllRefrences();
    }

    [Rpc(SendTo.Everyone , InvokePermission = RpcInvokePermission.Everyone)]
    public void AddEscapeToAllRpc()
    {
        foreach (var player in Players)
        {
            if (player.playerId == NetworkManager.LocalClientId)
            {
                escapeMenu.player = player.player.gameObject;
                escapeMenu.GetAllRefrences();
            }
        }
    }



public void BackToLobby()
    {
        escapeMenu.Disconnect();
        disconnected = true;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (NetworkManager.Singleton.ShutdownInProgress && !disconnected)
        {
            disconnected = true;
            escapeMenu.Disconnect();
        }
    }

    [Rpc(SendTo.Server , InvokePermission = RpcInvokePermission.Everyone)]
    public void PlayerDamageServerRpc(Vector3 playerDeathPosition , ulong playerId , NetworkObjectReference playerRef , bool IsOcean)
    {
        ActivePlayer damaged_player = Players.Find(player => player.playerId == playerId);
        if (!damaged_player.IsImmune)
        {
            damaged_player.IsImmune = true;
            StartCoroutine(WaitUnImmune(playerId));
            if (IsOcean)
            {
                MakeSeaKnockackServerRpc(damaged_player.player.GetComponent<NetworkObject>());
            }
            damaged_player.playerHealth.TakeDamageServerRpc();
            Debug.Log("Damage");
            if (damaged_player.playerHealth.currentHealth.Value == 0)
            {
                PlayerDiesServerRpc(playerDeathPosition, playerId, playerRef);
            }
        }
    }

    IEnumerator WaitUnImmune(ulong playerId)
    {
        yield return new WaitForSeconds(1.5f);
        UnImmuneRpc(playerId);
    }
    
    [Rpc(SendTo.Server , InvokePermission = RpcInvokePermission.Everyone)]
    private void UnImmuneRpc(ulong playerId)
    {
        ActivePlayer damaged_player = Players.Find(player => player.playerId == playerId);
        damaged_player.IsImmune = false;
    }

    [Rpc(SendTo.Everyone)]
    private void MakeSeaKnockackServerRpc(NetworkObjectReference playerO)
    {
        playerO.TryGet(out NetworkObject player);
        player.GetComponent<PlayerKnockbackSystem>().SeaKnockback(OceanKnockBackForce);
    }
    
    [Rpc(SendTo.Server , InvokePermission = RpcInvokePermission.Everyone)]
    public void PlayerDiesServerRpc(Vector3 playerDeathPosition , ulong playerId , NetworkObjectReference playerRef)
    {
        GameObject spectatorPlayer = Instantiate(SpectatorPlayer.gameObject, playerDeathPosition, Quaternion.identity);
        spectatorPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(playerId, true);
        playerRef.TryGet(out NetworkObject player);
        player.Despawn();
        ActivePlayer deadplayer = Players.Find(player => player.playerId == playerId);
        deadplayer.player = spectatorPlayer.transform;
        deadplayer.isAlive = false;
        CheckForWinnerServerRpc();
    }

    public void KickPlayer()
    {
        //ForFutureUse
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
            PlayerWinsManager winsManager = FindFirstObjectByType<PlayerWinsManager>();
            winsManager.AddCurrentPlayerByPlayerIdRpc(winnerPlayerId);
            SetCurrentWinnersServerRpc();
            ActivePlayer winnerPlayer = Players.Find(player => player.playerId == winnerPlayerId);
            SetWinnerCameraLockModeClientRpc(winnerPlayer.player.GetComponent<NetworkObject>());
            winnerPlayer.isWinner = true;
            ShowWinnerServerRpc();
            //Go To Winner Screen
            if (winsManager.CheckIfAnyoneWon())
            {
                PlayerData playerData = FindFirstObjectByType<ReadyUp>().GetPlayerDataFromClientId(winnerPlayerId);
                SetWinnerNameRpc(playerData.name.ToString());
                GoToWinnerScene();
            }
        }
        else
        {
            Debug.Log("no one won yet");
        }
    }

    [Rpc(SendTo.Everyone)]
    private void SetWinnerNameRpc(string name)
    {
        FindFirstObjectByType<PlayerWinsManager>().WinnerName = name;
    }

    [ClientRpc]
    private void SetWinnerCameraLockModeClientRpc(NetworkObjectReference winnerPlayerRef)
    {
        winnerPlayerRef.TryGet(out NetworkObject winnerPlayer);
        winnerPlayer.GetComponent<CameraController>().CanMoveCamera = false;
    }

    [ClientRpc]
    private void UpdateWinStateCanvasClientRpc(bool HasWon , ClientRpcParams RpcParams = default)
    {
        if (HasWon)
        {
            //turn on winner canvas ui
            WinUI.SetActive(true);
            WinnersUI.SetActive(true);
        }
        else
        {
            //You Lose canvas ui
            LoseUI.SetActive(true);
            WinnersUI.SetActive(true);
        }
    }

    [Rpc(SendTo.Server , InvokePermission = RpcInvokePermission.Everyone)]
    private void ShowWinnerServerRpc()
    {
        ActivePlayer Winner = Players.Find(Players => Players.isWinner);
        Winner.player.GetComponent<CharecterController>().CanMove = false;
        
        List<ActivePlayer> allDeadPlayers = new List<ActivePlayer>();
        foreach (ActivePlayer deadplayer in Players)
        {
            if (!deadplayer.isAlive)
            {
                allDeadPlayers.Add(deadplayer);
            }
        }

        foreach (var player in allDeadPlayers)
        {
            player.player.transform.position = Winner.player.transform.position;
        }
        
        foreach (var player in Players)
        {
            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { player.playerId }
                }
            }; 
            bool HasWon = player.isWinner;
            UpdateWinStateCanvasClientRpc(HasWon , clientRpcParams);
        }

        GameOverRpc();
    }

    private void GoToWinnerScene()
    {
        FindFirstObjectByType<ReadyUp>().ChangeScenesRpc(Loader.Scene.WinScreen);
    }

    [ServerRpc]
    private void SetCurrentWinnersServerRpc()
    {
        for (int i = 0; i < Players.Count; i++)
        {
            PlayerData playerData = FindFirstObjectByType<ReadyUp>().GetPlayerDataFromClientId(Players[i].playerId);
            SetCurrentWinnersForEveryOneRpc(playerData.name.ToString() , FindFirstObjectByType<PlayerWinsManager>().GetCurrentWinsByPlayerId(Players[i].playerId));
        }
    }

    [Rpc(SendTo.Everyone , InvokePermission = RpcInvokePermission.Everyone)]
    private void SetCurrentWinnersForEveryOneRpc(string playerName , int currentWins)
    {
        GameObject prefab = Instantiate(PlayerPointPrefab, playerPointContent);
        prefab.GetComponent<PlayerPoints>().pointsText.text = playerName + ": " + currentWins + "/3 Wins";
    }
    
    [Rpc(SendTo.Everyone , InvokePermission = RpcInvokePermission.Everyone)]
    private void GameOverRpc()
    {
        GameOver = true;
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

    private bool PressedGoBackModifier;
    public void GoToModifierSelect()
    {
        Invoke(nameof(SimpleDelayModifierSelect) , 0.1f);
    }

    private void SimpleDelayModifierSelect()
    {
        Debug.Log("PressedGoToModifierSelect");
        if (!PressedGoBackModifier)
        {
            FindFirstObjectByType<ReadyUp>().ChangeScenesRpc(Loader.Scene.ChooseModifier);
            PressedGoBackModifier = true;
        }
    }
}
