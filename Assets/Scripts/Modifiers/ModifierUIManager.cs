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
        public int selectedIndex;
        public string playerName;
    }
    
    [SerializeField] private GameObject modifier;
    [SerializeField] private GameObject playeready;
    [SerializeField] private Transform Content;
    [SerializeField] private Transform playerReadyContent;
    
    [SerializeField] private AllModifiersHolderScriptableObject allModifiersHolder;
    private ModifierScriptableObject[] allModifiers;
    [SerializeField] private List<int> CurrentModifiers;
    [SerializeField] private List<readiedPlayer> playerReadied;
    [SerializeField] private List<GameObject> readyPlayersObject;

    private bool canCheck = false;
    private int currentSpawnedModifiers = 0;

    [SerializeField] private int firstIndex;
    [SerializeField] private int secondIndex;

    private void Awake()
    {
        allModifiers = allModifiersHolder.AllModifiers;
    }

    public override void OnNetworkSpawn()
    {
        SetCurrentModifiers(FindFirstObjectByType<ModifierHolder>().activeModifiers);
        if (IsHost)
            Create2Modifier();
        if (!IsHost) return;
        Debug.Log("ModifierMenuSpawn Conencted Players = " + NetworkManager.Singleton.ConnectedClientsIds.Count);
        GetPlayerReadyRpc();
    }


    [SerializeField] private List<GameObject> current_modifiers;

    [Rpc(SendTo.Everyone)]
    public void SetTwoCurrentModifiersWithIdexesRpc(int firstIndex, int secondIndex)
    {
        if (IsHost) return;
        GameObject mod = Instantiate(modifier , Content);
        mod.GetComponent<Modifier>().Setmodifier(allModifiers[firstIndex]);
        current_modifiers.Add(mod);
        CurrentModifiers.Add(firstIndex);
        
        GameObject mod2 = Instantiate(modifier , Content);
        mod2.GetComponent<Modifier>().Setmodifier(allModifiers[secondIndex]);
        current_modifiers.Add(mod2);
        CurrentModifiers.Add(secondIndex);
    }

    private bool HasUpdated = false;
    private void UpdateReady()
    {
        /*
        foreach (GameObject t in Content1)
        {
            Destroy(t);
        }
        foreach (GameObject t in Content2)
        {
            Destroy(t);
        }
        */
        Debug.Log("HasUpdated");
        foreach (var player in playerReadied)
        {
            if (player.isReady) return;
            if(!HasUpdated) return;
            if (player.selectedIndex == firstIndex)
            {
                SetPlayerReadyRpc(true, player.playerName);
            }
            else
            {
                SetPlayerReadyRpc(false, player.playerName);
            }
        }
        HasUpdated = true;
    }

    [SerializeField] private Transform Content1;
    [SerializeField] private Transform Content2;
    
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void SetPlayerReadyRpc(bool selectedFirstModifier, string playerName)
    {
        if (selectedFirstModifier)
        {
            GameObject _playerReadyHolder = Instantiate(playeready, Content1);
            readyPlayersObject.Add(_playerReadyHolder);
            _playerReadyHolder.GetComponent<ModifierPlayerReadyHolder>().text.text = playerName;
        }
        else
        {
            GameObject _playerReadyHolder = Instantiate(playeready, Content2);
            readyPlayersObject.Add(_playerReadyHolder);
            _playerReadyHolder.GetComponent<ModifierPlayerReadyHolder>().text.text = playerName;
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

    private int winnerIndex;
    
    public void ReadyUp(int selectedIndex)
    {
        ReadyThePlayerRpc(NetworkManager.Singleton.LocalClientId , selectedIndex);
        foreach (var modifer in current_modifiers)
        {
            modifer.GetComponent<Modifier>().setCantPlace();
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ReadyThePlayerRpc(ulong playerId , int selectedIndex)
    {
        readiedPlayer readyThisPlayer = playerReadied.Find(p => p.playerId == playerId);
        if (readyThisPlayer != null)
        {
            readyThisPlayer.isReady = true;
            readyThisPlayer.selectedIndex = selectedIndex;
            Debug.Log(readyThisPlayer.playerName + " is ready");
            if (readyThisPlayer.selectedIndex == firstIndex)
            {
                SetPlayerReadyRpc(true, readyThisPlayer.playerName);
            }
            else
            {
                SetPlayerReadyRpc(false, readyThisPlayer.playerName);
            }
            UpdateReady();
        }

        if (IsHost && GetIfAllPlayersReady())
        {
            canCheck = false;
            ModifierHolder holder = FindFirstObjectByType<ModifierHolder>();

            int firstIndexPlayerSelected = 0;

            foreach (var player in playerReadied)
            {
                if (player.selectedIndex == firstIndex)
                {
                    firstIndexPlayerSelected++;
                }
            }
            int secondIndexPlayerSelected = 0;
            secondIndexPlayerSelected = playerReadied.Count - firstIndexPlayerSelected;
            if (firstIndexPlayerSelected > secondIndexPlayerSelected)
            {
                winnerIndex = firstIndex;
            }
            else if (secondIndexPlayerSelected == firstIndexPlayerSelected)
            {
                int random = Random.Range(0 , 1);
                if (random == 0)
                {
                    winnerIndex = secondIndex;
                }
                else
                {
                    winnerIndex = firstIndex;
                }
                Debug.Log("RANDOM");
            }
            else
            {
                winnerIndex = secondIndex;
            }
            
            holder.AddModfierWithIndexRpc(winnerIndex);
            Back();
        }
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

    public void Create2Modifier()
    {
        firstIndex = Random.Range(0 , allModifiers.Length);
        GameObject mod = Instantiate(modifier , Content);
        mod.GetComponent<Modifier>().Setmodifier(allModifiers[firstIndex]);
        CurrentModifiers.Add(firstIndex);

        CreateSecondModifier();
    }

    private void CreateSecondModifier()
    {
        int waitSecondIndex = Random.Range(0 , allModifiers.Length);
        if (waitSecondIndex == firstIndex)
        {
            CreateSecondModifier();
        }
        else
        {
            secondIndex = waitSecondIndex;
            GameObject mod1 = Instantiate(modifier , Content);
            mod1.GetComponent<Modifier>().Setmodifier(allModifiers[secondIndex]);
            CurrentModifiers.Add(secondIndex);
            SetTwoCurrentModifiersWithIdexesRpc(firstIndex , secondIndex);
        }
        
    }
}
