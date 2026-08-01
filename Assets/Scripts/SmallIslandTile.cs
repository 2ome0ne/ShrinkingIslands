using System;
using Unity.Netcode;
using UnityEngine;

public class SmallIslandTile : NetworkBehaviour
{
    [SerializeField] private GameObject[] ListOfGTXs;
    [SerializeField] private Transform[] ListOfForgeLocations;

    [SerializeField] private NetworkObject forge;

    public override void OnNetworkSpawn()
    {
        Set_a_GTXRpc();
    }

    [Rpc(SendTo.Everyone , InvokePermission = RpcInvokePermission.Everyone)]
    public void Set_a_GTXRpc()
    {
        if(!IsServer) return;
        forge.Spawn(true);
        forge.TrySetParent(this.transform);
    }
}
