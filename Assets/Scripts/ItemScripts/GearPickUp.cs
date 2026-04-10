using Unity.Netcode;
using UnityEngine;

public class GearPickUp : NetworkBehaviour
{
    public GearManager.Gear gear;

    public GameObject HoldItem;
    public float DespawnY = 40;
    private Rigidbody rb;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
    }

    [Rpc(SendTo.Server , InvokePermission = RpcInvokePermission.Everyone)]
    public void DestroyPickUpServerRpc()
    {
        NetworkObject.Despawn();
    }
    
    private void FixedUpdate()
    {
        if(IsServer)
            if (transform.position.y > DespawnY && !rb.isKinematic)
            {
                DespawnItemRpc();
            }
    }
    
    
    [Rpc(SendTo.Server)]
    private void DespawnItemRpc()
    {
        NetworkObject.Despawn(gameObject);
    }
}
