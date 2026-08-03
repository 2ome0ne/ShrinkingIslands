using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class LaunchMushroom : NetworkBehaviour
{
    [SerializeField] private NetworkAnimator animator;
    [SerializeField] private float PushPower = 100;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerKnockbackSystem>())
        {
            Debug.Log("LAUNCH MUSHROOM");
            GameManager.Instance.soundManager.SpawnSoundRpc(transform.position, 20 , 1 , 1 , 12);
             PlayerKnockbackSystem player = other.GetComponent<PlayerKnockbackSystem>();
             player.MushroomKnockback(PushPower);
             MushroomBounceRpc();
        }
    }

    [Rpc(SendTo.Everyone , InvokePermission = RpcInvokePermission.Everyone)]
    private void MushroomBounceRpc()
    {
        animator.SetTrigger("Push");
    }
}
