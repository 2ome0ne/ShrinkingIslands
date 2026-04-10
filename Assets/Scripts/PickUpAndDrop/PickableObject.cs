using System;
using Unity.Netcode;
using UnityEngine;

public class PickableObject : NetworkBehaviour
{
    public int ObjectIndex;
    private FollowTransform followTransform;
    [SerializeField] private float DespawnY = 20f;
    [SerializeField] private Rigidbody rb;

    private void Start()
    {
        followTransform = GetComponent<FollowTransform>();
        
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
