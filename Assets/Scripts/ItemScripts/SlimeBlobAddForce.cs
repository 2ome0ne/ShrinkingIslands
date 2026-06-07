using System;
using System.Globalization;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class SlimeBlobAddForce : NetworkBehaviour
{
    [Header("--[Settings]--")] [SerializeField]
    private float KbDuration;
    [SerializeField] private float KbForce;

    [SerializeField] private LayerMask HittableLayer;
    [SerializeField] private float MaxDespawnTime;
    private float CurrentDespawnTime;

    private void Start()
    {
        if (!IsServer)
        {
            return;
        }

        CurrentDespawnTime = MaxDespawnTime;
    }

    private void Update()
    {
        if(!IsServer) return;
        CurrentDespawnTime -= Time.deltaTime;
        if (CurrentDespawnTime <= 0)
        {
            DespawnBlobServerRpc();
        }
    }

    [ServerRpc]
    private void DespawnBlobServerRpc()
    {
        NetworkObject.Despawn(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        //if(!IsServer) return;
        if (other.GetComponent<PlayerKnockbackSystem>())
        {
            other.GetComponent<PlayerKnockbackSystem>().KnockBack(this.transform.position , KbForce , null);
        }
    }
}
