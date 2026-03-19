using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class TimeBomb : NetworkBehaviour
{
    [Header("--[References]--")] [SerializeField]
    private float MaxCooldown;
    
    public NetworkVariable<float> CurrentCooldown;
    [SerializeField] private GameObject ExplosionEffect;
    [SerializeField] private PickUpSystem PickUpSystem;

    public override void  OnNetworkSpawn()
    {
        if(IsServer)
            CurrentCooldown.Value = MaxCooldown;
    }

    private void FixedUpdate()
    {
        if (IsServer)
        {
            CurrentCooldown.Value -= Time.fixedDeltaTime;

            if (CurrentCooldown.Value <= 0)
            {
                PickUpSystem = this.GameObject().GetComponent<FollowTransform>().player.GetComponent<PickUpSystem>();
                if (PickUpSystem.CurrentHoldObject == this.transform)
                {
                    PickUpSystem.DePick();
                }
                ExplodeServerRpc();
            }
        }
    }

    [ServerRpc]
    void ExplodeServerRpc()
    {
        NetworkObject Obj = Instantiate(ExplosionEffect, transform.position, Quaternion.identity).GetComponent<NetworkObject>();
        Obj.Spawn();
        this.NetworkObject.Despawn(true);
    }
}
