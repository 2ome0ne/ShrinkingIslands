using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ModifierManager : NetworkBehaviour
{
    [System.Serializable]
    public class Modifier
    {
        public string name;
        public bool Enabled;
    }
    
    [Header("References")] 
    public List<ModifierScriptableObject> ActiveModifiers;
    [SerializeField] private GameManager _gameManager;
    public PropSpawner propSpawner;
    [SerializeField] private RandomlySpawnItems randomlySpawnItems;
    [SerializeField] private AllModifiersHolderScriptableObject allModifiersHolder;
    [SerializeField] private List<Modifier> AllModifiersEnabled;

    [Header("Modifiers")]
    //Fog
    [SerializeField]
    private float defaultfog = 0.01f;

    [SerializeField] private float AlotOfFogAmount = 0.1f;
    
    //Lighting Strike
    [SerializeField] private bool ThunderStormEnabled = false;
    private bool WaitForWarning = false;
    [SerializeField] private float MinLightingStrike;
    [SerializeField] private float MaxLightingStrike;

    [SerializeField] private GameObject WarningEffect;
    [SerializeField] private GameObject LightingPrefab;
    [SerializeField] private Transform TargetTransform;
    private float currentLightingStrikeCooldown;
    private float currentWarningEffectCooldown;
    private GameObject currentWarningEffect;
    
    
    // to make a modifier you have to set a if statement and check if the index is enabled to execute the modifier

    private void Awake()
    {
        foreach (var all_modifier in allModifiersHolder.AllModifiers)
        {
            Modifier new_modifier = new Modifier();
            new_modifier.name = all_modifier.modifierName;
            AllModifiersEnabled.Add(new_modifier);
        }
    }

    private void Update()
    {
        if (ThunderStormEnabled && IsServer)
        {
            if (!WaitForWarning)
            {
                currentLightingStrikeCooldown -= Time.deltaTime;
                if (currentLightingStrikeCooldown <= 0)
                {
                    TargetTransform = _gameManager.Players[GetRandomPlayer()].player;
                    GameObject warningEffect = Instantiate(WarningEffect, TargetTransform.position, Quaternion.identity);
                    Debug.Log("WORKING");
                    warningEffect.GetComponent<NetworkObject>().Spawn();
                    warningEffect.GetComponent<NetworkObject>().TrySetParent(TargetTransform);
                    currentWarningEffect = warningEffect;
                    currentWarningEffectCooldown = 4;
                    WaitForWarning = true;
                }
            }
            else
            {
                currentWarningEffectCooldown -= Time.deltaTime;
                if (currentWarningEffectCooldown <= 0.7f)
                {
                    currentWarningEffect.GetComponent<NetworkObject>().TryRemoveParent(true);
                }
                if (currentWarningEffectCooldown <= 0)
                {
                    WaitForWarning = false;
                    currentLightingStrikeCooldown = UnityEngine.Random.Range(MinLightingStrike, MaxLightingStrike);
                    GameObject lighting = Instantiate(LightingPrefab, currentWarningEffect.transform.position, Quaternion.identity);
                    currentWarningEffect.GetComponent<NetworkObject>().Despawn();
                    lighting.GetComponent<NetworkObject>().Spawn();
                }
            }
        }
    }

    private int GetRandomPlayer()
    {
        List<GameManager.ActivePlayer> players = new List<GameManager.ActivePlayer>();
        foreach (var player in _gameManager.Players)
        {
            if (player.isAlive)
            {
                players.Add(player);
            }
        }
        
        return UnityEngine.Random.Range(0, players.Count);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsHost) return;
        ModifierHolder holder = FindFirstObjectByType<ModifierHolder>();
        foreach (var activeModifier in holder.activeModifiers)
        {
            ActiveModifiers.Add(activeModifier);
            EnableModifierByName(activeModifier.modifierName);
        }
        
        
        if(CheckEnabledModifierByName("Gum Rock"))
            GumRockSpawn();
        
        if (CheckEnabledModifierByName("Mountains"))
            MoutainsSpawn();
        if (CheckEnabledModifierByName("Flint lock"))
            FlintLockActivate();
        if (CheckEnabledModifierByName("Mine Field"))
            LandMineActive();
        if (CheckEnabledModifierByName("Thunder Storm"))
            ThunderStormActivate();
        if (CheckEnabledModifierByName("Harpoon Time"))
            HarpoonTimeActivate();
        RenderSettings.fogDensity = defaultfog;
        if (CheckEnabledModifierByName("Alot Of Fog"))
            AlotOfFogActivateRpc();
    }

    private void EnableModifierByName(string name)
    {
        foreach (var modifier in AllModifiersEnabled)
        {
            if (modifier.name == name)
            {
                modifier.Enabled = true;
            }
        }
    }

    private bool CheckEnabledModifierByName(string name)
    {
        bool result = false;
        foreach (var modifier in AllModifiersEnabled)
        {
            if (modifier.name == name && modifier.Enabled)
            {
                result = true;
            }
        }
        return result;
    }

    public void GumRockSpawn()
    {
        Debug.Log("Gum Rock Spawn");
        propSpawner.EnablePropByIndex(2);
    }
    
    public void MoutainsSpawn()
    {
        Debug.Log("Mountains Spawn");
        propSpawner.EnablePropByIndex(3);
        propSpawner.EnablePropByIndex(4);
        propSpawner.EnablePropByIndex(5);
        propSpawner.EnablePropByIndex(6);
    }

    public void FlintLockActivate()
    {
        Debug.Log("Flint Lock Activate");
        randomlySpawnItems.EnableItemToSpawnByIndexServerRpc(3);
    }

    [Rpc(SendTo.Everyone , InvokePermission = RpcInvokePermission.Everyone)]
    public void AlotOfFogActivateRpc()
    {
        RenderSettings.fogDensity = AlotOfFogAmount;
    }

    public void LandMineActive()
    {
        propSpawner.EnablePropByIndex(7);
    }

    public void ThunderStormActivate()
    {
        ThunderStormEnabled = true;
    }

    public void HarpoonTimeActivate()
    {
        randomlySpawnItems.EnableItemToSpawnByIndexServerRpc(4);
    }
}
