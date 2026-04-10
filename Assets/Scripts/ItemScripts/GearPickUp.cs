using Unity.Netcode;
using UnityEngine;

public class GearPickUp : NetworkBehaviour
{
    public GearManager.Gear gear;

    public GameObject HoldItem;

    [Rpc(SendTo.Server , InvokePermission = RpcInvokePermission.Everyone)]
    public void DestroyPickUpServerRpc()
    {
        NetworkObject.Despawn();
    }
}
