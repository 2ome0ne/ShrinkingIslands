
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    [SerializeField] private Transform sp1;
    [SerializeField] private Transform sp2;
    [SerializeField] private Transform playerReadyContent;

    [SerializeField] private int TimeUntilShow = 30;
    [SerializeField] private int TimeUntilChangeScenes = 5;

    [SerializeField] private Transform ShowSelectedModifier;
    [SerializeField] private TextMeshProUGUI modifierText;
    [SerializeField] private TextMeshProUGUI TimerText;
    [SerializeField] private Image modifierIcon;
    
    [SerializeField] private AllModifiersHolderScriptableObject allModifiersHolder;
    private ModifierScriptableObject[] allModifiers;
    [SerializeField] private List<int> CurrentModifiers;
    [SerializeField] private List<readiedPlayer> playerReadied;
    [SerializeField] private List<GameObject> readyPlayersObject;

    private bool playersVoted = false;
    private bool canCheck = false;
    private int currentSpawnedModifiers = 0;
    
    [SerializeField] private NetworkVariable<int> currentTimer;

    [SerializeField] private bool IsInShow = false;

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
        StartCoroutine(waitSetTwoCurrentModifiersWithIdexes(firstIndex , secondIndex));
    }

    IEnumerator waitSetTwoCurrentModifiersWithIdexes(int firstIndex, int secondIndex)
    {
        if (IsHost) yield break;
        GameObject mod = Instantiate(modifier , sp1);
        mod.GetComponent<Modifier>().Setmodifier(allModifiers[firstIndex]);
        current_modifiers.Add(mod);
        CurrentModifiers.Add(firstIndex);
        
        yield return new WaitForSeconds(0.5f);
        
        GameObject mod2 = Instantiate(modifier , sp2);
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
        ResetEveryonesVotesRpc();
        foreach (var player in playerReadied)
        {
            if (!player.isReady) return;
            //if(!HasUpdated) return;
            if (player.selectedIndex == firstIndex)
            {
                SetPlayerReadyRpc(true, player.playerName);
            }
            else
            {
                SetPlayerReadyRpc(false, player.playerName);
            }
        }
        //HasUpdated = true;
    }

    [Rpc(SendTo.Everyone)]
    private void ResetEveryonesVotesRpc()
    {
        foreach (Transform votes in Content1.transform)
        {
            Destroy(votes.gameObject);
        }
        foreach (Transform votes in Content2.transform)
        {
            Destroy(votes.gameObject);
        }
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
            //_playerReadyHolder.GetComponent<ModifierPlayerReadyHolder>().text.text = playerName;
        }
        else
        {
            GameObject _playerReadyHolder = Instantiate(playeready, Content2);
            readyPlayersObject.Add(_playerReadyHolder);
            //_playerReadyHolder.GetComponent<ModifierPlayerReadyHolder>().text.text = playerName;
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
    }
    
    public void resetOthers()
    {
        resetOthersRpc();
    }

    [Rpc(SendTo.Me , InvokePermission = RpcInvokePermission.Everyone)]
    public void resetOthersRpc()
    {
        foreach (var modifer in current_modifiers)
        {
            modifer.GetComponent<Modifier>().UnPress();
        }
    }

    private bool shortcuted = false;

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

        //to skip
        if (GetIfAllPlayersReady() && !shortcuted && currentTimer.Value > 5)
        {
            shortcuted = true;

            currentTimer.Value = 5;
        }

        //GetWinnerModifier();
    }

    private void GetWinnerModifier()
    {
        if (IsHost && GetIfAllPlayersReady())
        {
            canCheck = false;

            int firstIndexPlayerSelected = 0;

            foreach (var player in playerReadied)
            {
                if (player.selectedIndex == firstIndex)
                {
                    firstIndexPlayerSelected++;
                }
            }
            Debug.Log("AFPC = " + firstIndexPlayerSelected);
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
            
            playersVoted = true;
        }
    }

    [Rpc(SendTo.Everyone)]
    public void UpdateShowForEveryOneRpc(int WI)
    {
        ModifierHolder holder = FindFirstObjectByType<ModifierHolder>();
        if(IsServer)
            holder.AddModfierWithIndexRpc(WI);
        modifierIcon.sprite = holder.GetModifierByIndex(WI).modifierIcon;
        modifierText.text = "Modifier: " + holder.GetModifierByIndex(WI).modifierName;
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
    //ModifierHolder
    public void Create2Modifier()
    {
        firstIndex = Random.Range(0 , allModifiers.Length);
        GameObject mod = Instantiate(modifier , sp1);
        current_modifiers.Add(mod);
        mod.GetComponent<Modifier>().Setmodifier(allModifiers[firstIndex]);
        CurrentModifiers.Add(firstIndex);

        Invoke("CreateSecondModifier" , 0.5f);
    }

    private void CreateSecondModifier()
    {
        int waitSecondIndex = Random.Range(0 , allModifiers.Length);
        if (waitSecondIndex == firstIndex)
        {
            CreateSecondModifier();
            return;
        }
        else
        {
            secondIndex = waitSecondIndex;
            GameObject mod1 = Instantiate(modifier , sp2);
            current_modifiers.Add(mod1);
            mod1.GetComponent<Modifier>().Setmodifier(allModifiers[secondIndex]);
            CurrentModifiers.Add(secondIndex);
            SetTwoCurrentModifiersWithIdexesRpc(firstIndex , secondIndex);
        }
        currentTimer.Value = TimeUntilShow;
        Debug.Log("current Timer is " + currentTimer.Value);
        StartCoroutine(timerUpdate());
    }

    IEnumerator timerUpdate()
    {
        yield return new WaitForSeconds(1);
        currentTimer.Value--;
        Debug.Log("REBOUND");
        if (currentTimer.Value == 0)
        {
            if (!IsInShow)
            {
                Debug.Log("show selected modifier");
                ShowToEveryOneRpc();
                GetWinnerModifier();
                IsInShow = true;
                currentTimer.Value = TimeUntilChangeScenes;
                StartCoroutine(timerUpdate());
                UpdateShowForEveryOneRpc(winnerIndex);
                yield break;
            }
            else if(IsInShow)
            {
                Debug.Log("BACK");
                Back();
            }
        }

        UpdateTimerTextEveryoneRpc();
        StartCoroutine(timerUpdate());
    }

    [Rpc(SendTo.Everyone , InvokePermission = RpcInvokePermission.Everyone)]
    private void UpdateTimerTextEveryoneRpc()
    {
        TimerText.text = currentTimer.Value.ToString();
    }

    [Rpc(SendTo.Everyone , InvokePermission = RpcInvokePermission.Everyone)]
    private void ShowToEveryOneRpc()
    {
        Debug.Log("ShowToEveryOneRpc");
        ShowSelectedModifier.gameObject.SetActive(true);
    }
    
    
}
