using System;
using Unity.Netcode;
using UnityEngine;

public class LaunchMushroom : NetworkBehaviour
{
    [SerializeField] private ownerNetworkAnimator animator;
    [SerializeField] private float PushPower = 100;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerKnockbackSystem>())
        {
            Debug.Log("LAUNCH MUSHROOM");
            GameManager.Instance.soundManager.SpawnSoundRpc(transform.position, 20 , 1 , 1 , 12);
             PlayerKnockbackSystem player = other.GetComponent<PlayerKnockbackSystem>();
             animator.SetTrigger("Push");
             player.MushroomKnockback(PushPower);
        }
    }
}
