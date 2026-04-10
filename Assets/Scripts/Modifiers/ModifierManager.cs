using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ModifierManager : NetworkBehaviour
{
    [System.Serializable]
    public class Modifier
    {
        public string name;
        public int index;
        public bool Enabled;
    }
    
    [Header("References")] 
    public List<ModifierScriptableObject> ActiveModifiers;
    public PropSpawner propSpawner;
    [SerializeField] private RandomlySpawnItems randomlySpawnItems;
    public Modifier[] AllModifiers;
    
    // to make a modifier you have to set a if statement and check if the index is enabled to execute the modifier

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
    }

    private void EnableModifierByName(string name)
    {
        foreach (var modifier in AllModifiers)
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
        foreach (var modifier in AllModifiers)
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
}
