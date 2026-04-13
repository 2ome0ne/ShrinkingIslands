using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ModifierUIManager : NetworkBehaviour
{
    [System.Serializable]
    public class readiedPlayer
    {
        public ulong playerId;
        public bool isReady;
        public string playerName;
    }
    
    [SerializeField] private GameObject modifier;
    [SerializeField] private GameObject playeready;
    [SerializeField] private Transform Content;
    [SerializeField] private Transform playerReadyContent;
    
    [SerializeField] private ModifierScriptableObject[] allModifiers;
    [SerializeField] private List<int> CurrentModifiers;
    [SerializeField] private List<readiedPlayer> playerReadied;
    [SerializeField] private List<GameObject> readyPlayersObject;

    private bool canCheck = false;
    private int currentSpawnedModifiers = 0;

    public override void OnNetworkSpawn()
    {
        SetCurrentModifiers(FindFirstObjectByType<ModifierHolder>().activeModifiers);
        Create3Modifier();
        if (!IsHost) return;
        Debug.Log("ModifierMenuSpawn Conencted Players = " + NetworkManager.Singleton.ConnectedClientsIds.Count);
        GetPlayerReadyRpc();
    }

    private void Update()
    {
        if (!IsHost) return;
        if(!canCheck) return;
        if (GetIfAllPlayersReady())
        {
            Back();
        }
    }

    private void UpdateReady()
    {
        foreach (GameObject t in readyPlayersObject)
        {
            Destroy(t);
        }
        foreach (var player in playerReadied)
        {
            GameObject _playerReadyHolder = Instantiate(playeready, playerReadyContent);
            readyPlayersObject.Add(_playerReadyHolder);
            string isready = new string("");
            if (player.isReady)
            {
                isready = "Ready";
            }
            else
            {
                isready = "Not Ready";
            }
            _playerReadyHolder.GetComponent<ModifierPlayerReadyHolder>().text.text = player.playerName + isready;
        }
    }

    private bool GetIfAllPlayersReady()
    {
        foreach (var player in playerReadied)
        {
            if (!player.isReady)
            {
                return false;
            }
        }
        return true;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void GetPlayerReadyRpc()
    {
        foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            PlayerData playerData = FindFirstObjectByType<ReadyUp>().GetPlayerDataFromClientId(id);
            readiedPlayer readiedPlayerid = new readiedPlayer(); 
            readiedPlayerid.playerId = id;
            readiedPlayerid.isReady = false;
            readiedPlayerid.playerName = playerData.name.ToString();
            playerReadied.Add(readiedPlayerid);
            UpdateReady();
            canCheck = true;
        }
    }
    
    public void ReadyUp()
    {
        ReadyThePlayerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ReadyThePlayerRpc(ulong playerId)
    {
        readiedPlayer readyThisPlayer = playerReadied.Find(p => p.playerId == playerId);
        readyThisPlayer.isReady = true;
        UpdateReady();
        Debug.Log(playerReadied.Find(p => p.playerId == playerId).playerName
                  + " is Ready");
    }

    public void Back()
    {
        FindFirstObjectByType<ReadyUp>().ChangeScenesRpc(Loader.Scene.GameScene);
    }

    public void SetCurrentModifiers(List<ModifierScriptableObject> activeModifiers)
    {
        foreach (ModifierScriptableObject mod in activeModifiers)
        {
            CurrentModifiers.Add(mod.indexValue);
        }
    }

    public void Create3Modifier()
    {
        int spawnedModifiers = 0;
        while (spawnedModifiers < 2)
        {
            int index = Random.Range(0 , allModifiers.Length);
            Debug.Log("Creating modifier" + spawnedModifiers + " " + index);
            if (!CurrentModifiers.Contains(index))
            {
                GameObject mod = Instantiate(modifier , Content);
                mod.GetComponent<Modifier>().Setmodifier(allModifiers[index]);
                CurrentModifiers.Add(index);
                spawnedModifiers++;
            }
        }
    }
}
