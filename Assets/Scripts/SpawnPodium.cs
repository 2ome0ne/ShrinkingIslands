using System;
using Unity.Netcode;
using UnityEngine;

public class SpawnPodium : NetworkBehaviour
{
    public Transform PlayerSpawnPoint;
    [SerializeField] private NetworkObject JumpMushroom;

    public override void OnNetworkSpawn()
    {
        JumpMushroom.Spawn(true);
        JumpMushroom.TrySetParent(this.transform);
        NetworkObject.DestroyWithScene = true;
    }
}
