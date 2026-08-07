using System;
using Unity.Netcode;
using UnityEngine;

public class SOIslandItemisland : NetworkBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private SOIslandTile tile;
    [SerializeField] private bool spawnedItem = false;

    public bool IsRare;
    public GameObject SpawnThisObject;
    [SerializeField] private GameObject spawnedItemPrefab;
    [SerializeField] private Transform RarePosition;
    [SerializeField] private GameObject RareParticle;

    private bool setPos;
    
    private void FixedUpdate()
    {
        if (IsServer)
        {
            if (IsRare && !setPos)
            {
                setPos = true;
                sendToEveryoneClientRpc();
            }
            
            if (tile.Spawned && SpawnThisObject && !spawnedItem)
            {
                spawnedItem = true;
                spawnedItemPrefab = Instantiate(SpawnThisObject , spawnPoint.position, spawnPoint.rotation);
                spawnedItemPrefab.GetComponent<NetworkObject>().Spawn(true);
                SetEveryonesSpawnItemNOGClientRpc();
            }
            else if (IsRare && SpawnThisObject && !spawnedItem)
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
                    GameManager.Instance.randomlySpawnItems.currentSpawn--;
                    GameManager.Instance.randomlySpawnItems.RemoveActiveIslandAtVector2Position(new Vector2(transform.position.x, transform.position.z));
                    crumbleThisIslandClientRpc();
                }
            }
        }
    }

    [ClientRpc]
    private void sendToEveryoneClientRpc()
    {
        tile.GoToSpawnPosition = RarePosition;
        tile.Spawned = true;
        RareParticle.SetActive(true);
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
