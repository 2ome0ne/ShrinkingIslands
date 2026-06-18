using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class IslandHeart : NetworkBehaviour
{
    [Header("Heart Settings")] 
    [SerializeField] private float MinIrrosionTime;
    [SerializeField] private float MaxIrrosionTime;
    
    [SerializeField] private int MaxIslandCrumblePerIrrosion = 3;
    
    public float CurrentIrrosionTime;
    
    [SerializeField] private float centerBiasPower;
    [SerializeField] private float minTileSpacing;
    //Max island Count dictates by IslandSrengh varible but +1
    [SerializeField] private int MaxIslandTileCount = 10;

    [SerializeField] private int IslandStrength = 10;

    public float IslandRadius;

    [SerializeField] private int CurrentIslandTileCount;

    [SerializeField] private float IslandSpawnY;
    
    
    [Header("Heart Refrences")] 
    [SerializeField] private GameObject IslandPrefab;
    [SerializeField] private SpawnManager _spawnManager;

    private int maxPlacementAttempts = 150;
    
    [SerializeField] private List<SOIslandTile> activeIslands;

    private void Update()
    {
        if (IsHost)
        {
            if (Input.GetKeyDown(KeyCode.E)) SpawnIslandTile();
            IrrosionUpdate();
        }
    }

    private void IrrosionUpdate()
    {
        CurrentIrrosionTime -= Time.deltaTime;
        if (CurrentIrrosionTime <= 0 && CurrentIslandTileCount >= 2)
        {
            int CrumbleIslandAmount = Random.Range(1, MaxIslandCrumblePerIrrosion);
            if(CrumbleIslandAmount >= CurrentIslandTileCount) return;
            for (int i = 0; i < CrumbleIslandAmount; i++)
            {
                int randomIsland = Random.Range(0, activeIslands.Count);
                Debug.Log("CRUMBLE ISLAND : " + randomIsland);
                activeIslands[randomIsland].CrumbleThisIsland();
            }
            GetRandomIrrosionTime();
        }
    }

    public override void OnNetworkSpawn()
    {
        if(!IsHost) return;
        GetRandomIrrosionTime();
        StartSpawn();
    }

    private void GetRandomIrrosionTime()
    {
        CurrentIrrosionTime = Random.Range (MinIrrosionTime , MaxIrrosionTime);
    }
    
    private void StartSpawn()
    {
        for (int i = 0; i < MaxIslandTileCount; i++)
        {
            SpawnOriginalIslandTile();
        }
        _spawnManager.GenerateComplete = true;
    }

    public void IslandCrumble(SOIslandTile islandTile)
    {
        activeIslands.Remove(islandTile);
        DestroyIslandTileServerRpc(islandTile.GetComponent<NetworkObject>());
        CurrentIslandTileCount--;
        SpawnIslandTile();
    }

    [ServerRpc]
    private void DestroyIslandTileServerRpc(NetworkObjectReference netObj)
    {
        Debug.Log("DESTROYED AN ISLAND");
        netObj.TryGet(out NetworkObject island);
        island.Despawn(true);
    }

    public void SpawnIslandTile()
    {
        if (CurrentIslandTileCount < MaxIslandTileCount && TryGetValidSpawnPoint(out Vector3 point))
        {
            Debug.Log("SPAWNING ISLAND");
            CurrentIslandTileCount++;
            SOIslandTile islandTile = Instantiate(IslandPrefab , point, Quaternion.identity).GetComponent<SOIslandTile>();
            islandTile.GetComponent<NetworkObject>().Spawn(true);
            islandTile.islandHeart = this;
            activeIslands.Add(islandTile);
        }
    }
    
    public void SpawnOriginalIslandTile()
    {
        if (CurrentIslandTileCount < MaxIslandTileCount && TryGetValidSpawnPoint(out Vector3 point))
        {
            CurrentIslandTileCount++;
            SOIslandTile islandTile = Instantiate(IslandPrefab , point, Quaternion.identity).GetComponent<SOIslandTile>();
            var NetObj = islandTile.GetComponent<NetworkObject>();
            NetObj.Spawn();
            SetSpawnedAsOriginalIslandRpc(NetObj);
            islandTile.islandHeart = this;
            activeIslands.Add(islandTile);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void SetSpawnedAsOriginalIslandRpc(NetworkObjectReference netObj)
    {
        netObj.TryGet(out NetworkObject island);
        island.GetComponent<SOIslandTile>().originalIsland = true;
    }


    public Vector3 GetRandomPointInCircle()
    {
        float angle = Random.Range(0f, Mathf.PI * 1.2f);

        // Pow > 1 biases distance toward 0 (center)
        float t = Random.value;
        float distance = Mathf.Pow(t, centerBiasPower) * IslandRadius;

        float x = Mathf.Cos(angle) * distance;
        float z = Mathf.Sin(angle) * distance;

        return new Vector3(x, IslandSpawnY, z);
    }
    
    public bool TryGetValidSpawnPoint(out Vector3 result)
    {
        Vector3 bestPoint = Vector3.zero;
        float bestMinDist = -1f;

        for (int i = 0; i < maxPlacementAttempts; i++)
        {
            Vector3 candidate = GetRandomPointInCircle();

            float nearestDist = float.MaxValue;
            foreach (var pos in activeIslands)
            {
                float d = Vector3.Distance(candidate, pos.transform.position);
                if (d < nearestDist) nearestDist = d;
            }

            float tileSpacing = Random.Range(minTileSpacing, minTileSpacing * 2);
            if (activeIslands.Count == 0 || nearestDist >= tileSpacing)
            {
                result = candidate;
                return true;
            }

            // Track best candidate in case all attempts fail
            if (nearestDist > bestMinDist)
            {
                bestMinDist = nearestDist;
                bestPoint = candidate;
            }
        }

        result = bestPoint;
        return false; // valid but crowded; spawner can decide to skip or proceed
    }
    
    private void UpdateIslandStrength()
    {
        MaxIslandTileCount = IslandStrength + 1;
    }
}
