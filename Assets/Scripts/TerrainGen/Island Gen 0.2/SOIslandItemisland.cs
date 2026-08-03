using System;
using Unity.Netcode;
using UnityEngine;

public class SOIslandItemisland : NetworkBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private SOIslandTile tile;
    [SerializeField] private bool spawnedItem = false;

    public GameObject SpawnThisObject;
    [SerializeField] private GameObject spawnedItemPrefab;

    private void FixedUpdate()
    {
        if (IsServer)
        {
            if (tile.Spawned && SpawnThisObject && !spawnedItem)
            {
                spawnedItem = true;
                spawnedItemPrefab = Instantiate(SpawnThisObject , spawnPoint.position, spawnPoint.rotation);
                spawnedItemPrefab.GetComponent<NetworkObject>().Spawn(true);
                SetEveryonesSpawnItemNOGClientRpc();
            }

            if (spawnedItem && !tile.Crumbling)
            {
                if (spawnedItemPrefab == null)
                {
                    crumbleThisIslandClientRpc();
                }
            }
        }
    }

    [ClientRpc]
    private void crumbleThisIslandClientRpc()
    {
        tile.CrumbleThisIsland();
    }

    [ClientRpc]
    private void SetEveryonesSpawnItemNOGClientRpc()
    {
        if (spawnedItemPrefab.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }
    }
}
