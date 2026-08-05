using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ModifierHolder : NetworkBehaviour
{
    public List<ModifierScriptableObject> activeModifiers;

    [SerializeField]
    private AllModifiersHolderScriptableObject allModifiers;

    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    [Rpc(SendTo.Server , InvokePermission = RpcInvokePermission.Everyone)]
    public void AddModfierWithIndexRpc(int index)
    {
        activeModifiers.Add(allModifiers.AllModifiers[index]);
    }

    public ModifierScriptableObject GetModifierByIndex(int index)
    {
        return allModifiers.AllModifiers[index];
    }
}
