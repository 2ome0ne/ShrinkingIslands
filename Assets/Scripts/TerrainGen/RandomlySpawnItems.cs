using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomlySpawnItems : NetworkBehaviour
{
    [System.Serializable]
    public class spawnItem
    {
        public GameObject prefab;
        [Range(0 , 1)]
        public float probability; 
        public bool ableToSpawn = true;
        //the higher it is the more common it is
    }

    
    [SerializeField] private spawnItem[] spawnableItems;
    [SerializeField] private spawnItem[] spawnableRareItems;
    [SerializeField] private GameObject SpawnIndicator;
    [SerializeField] private IslandHeart islandHeart;
    [SerializeField] private TheSea theSea;
    [SerializeField] private GameObject IslandItemPrefab;
    [Header("--[Settings]--")] 
    public int MaxCanSpawn;
    public int currentSpawn;
    public float minSpawnTime;
    public float maxSpawnTime;
    //if there is any islands near it don't allow to spawn
    public float checkGroundNearRadius = 0.4f;
    public float itemIslandRadius;
    [SerializeField] private float centerpushPower;
    [SerializeField] private float reduceRadius = 5;
    [SerializeField] private float UpPushForce;

    [SerializeField] private float StartSpawnDelayTime = 10f;
    private float currentSpawnTime;
    
    public List<Vector2> currentActiveIslandPositions;

    private TideManager tideManager;
    private void Start()
    {
        currentSpawnTime = StartSpawnDelayTime;
        tideManager = GetComponent<TideManager>();
    }

    private void Update()
    {
        if (!IsServer) return;
        
        currentSpawnTime -= Time.deltaTime;
        if (currentSpawnTime <= 0)
        {
            TrySpawnItem();
        }
        
    }

    [ServerRpc]
    public void EnableItemToSpawnByIndexServerRpc(int index)
    {
        Debug.Log(index + " is enabled");
        spawnableItems[index].ableToSpawn = true;
    }

    private void TrySpawnItem()
    {
        if (currentSpawn > MaxCanSpawn)
            return;
        float random = Random.Range(0, 10);
        Debug.Log("TSI" + random);
        bool isRare = random <= 2f;
        int index = 0;
        if (isRare)
        {
            index = GetRandomRareItemIndex();
        }
        else
        {
            index = GetRandomItemIndex();
        }
        
        if (CalculateIfItemSpawns(index))
        {
            //spawn item
            if (checkIfRandomPointInWater(out Vector3 spawnPos , isRare))
            {
                var island = Instantiate(IslandItemPrefab, spawnPos, Quaternion.identity);
                if (isRare)
                {
                    island.GetComponent<SOIslandItemisland>().IsRare = true;
                    island.GetComponent<SOIslandItemisland>().SpawnThisObject = spawnableRareItems[index].prefab;
                }
                else
                {
                    island.GetComponent<SOIslandItemisland>().SpawnThisObject = spawnableItems[index].prefab;
                }
                island.GetComponent<NetworkObject>().Spawn(true);
                currentSpawn++;
                //Spawn Push force
            }
        }
        currentSpawnTime = Random.Range(minSpawnTime, maxSpawnTime);
    }

    [Rpc(SendTo.Everyone)]
    private void SpawnTheIndicatorServerRpc(Vector3 spawnPos)
    {
        GameObject spawnindicator = Instantiate(SpawnIndicator , spawnPos , Quaternion.identity);
        Destroy(spawnindicator , 4);
    }

    [ServerRpc]
    private void SetNoGravityToItemServerRpc(NetworkObjectReference refObj)
    {
        refObj.TryGet(out NetworkObject networkObj);
        networkObj.GetComponent<Rigidbody>().useGravity = false;
    }
    
    [Header("--[spawnPoints]--")]
    [SerializeField] private Transform spt1;
    [SerializeField] private Transform spt2;

    [SerializeField] private LayerMask GroundLayer;
    
    private bool checkIfRandomPointInWater(out Vector3 spawnPos , bool isRare)
    {
        spawnPos = Vector3.zero;
        if (isRare)
        {
            int retryTimes = 0;
            bool hasHitAnyIslands = true;
            while (retryTimes < 20 && hasHitAnyIslands)
            {
                hasHitAnyIslands = false;
                spawnPos = GetRandomPointInCircleLowest();
                foreach (var island in islandHeart.activeIslands)
                {
                    Vector2 islandPos = new Vector2(island.transform.position.x, island.transform.position.z);
                    if ((new Vector2(spawnPos.x, spawnPos.z) - islandPos).sqrMagnitude <
                        (checkGroundNearRadius + itemIslandRadius) * (checkGroundNearRadius + itemIslandRadius))
                    {
                        hasHitAnyIslands = true;
                        break;
                    }
                }
                foreach (var island in currentActiveIslandPositions)
                {
                    if ((new Vector2(spawnPos.x, spawnPos.z) - island).sqrMagnitude <
                        (checkGroundNearRadius * 0.4f) * (checkGroundNearRadius * 0.4f))
                    {
                        hasHitAnyIslands = true;
                        break;
                    }
                }
                retryTimes++;
            }

            if (!hasHitAnyIslands)
            {
                currentActiveIslandPositions.Add(new Vector2(spawnPos.x, spawnPos.z));
                return true;
            }
        }
        else
        {
            spawnPos = Vector3.zero;
            int retryTimes = 0;
            bool hasHitAnyIslands = true;
            while (retryTimes < 10 && hasHitAnyIslands)
            {
                hasHitAnyIslands = false;
                spawnPos = GetRandomPointInCircle();
                foreach (var island in currentActiveIslandPositions)
                {
                    if ((new Vector2(spawnPos.x, spawnPos.z) - island).sqrMagnitude <
                        (checkGroundNearRadius * 0.5f) * (checkGroundNearRadius * 0.5f))
                    {
                        hasHitAnyIslands = true;
                        break;
                    }
                }
                retryTimes++;
            }

            if (!hasHitAnyIslands)
            {
                currentActiveIslandPositions.Add(new Vector2(spawnPos.x, spawnPos.z));
                return true;
            }
        }

        return false;
    }

    public void RemoveActiveIslandAtVector2Position(Vector2 islandPos)
    {
        //var found = currentActiveIslandPositions.Find(x => x.Equals(islandPos));
        currentActiveIslandPositions.Remove(islandPos);
    }
    
    private Vector3 GetRandomPointInCircle()
    {
        float angle = Random.Range(0f, Mathf.PI * 1.2f);

        // Pow > 1 biases distance toward 0 (center)
        float t = Random.value;
        float distance = Mathf.Pow(t, centerpushPower) * islandHeart.IslandRadius - reduceRadius;

        float x = Mathf.Cos(angle) * distance;
        float z = Mathf.Sin(angle) * distance;

        return new Vector3(x, theSea.transform.position.y, z);
    }
    
    private Vector3 GetRandomPointInCircleLowest()
    {
        float angle = Random.Range(0f, Mathf.PI * 1.2f);

        // Pow > 1 biases distance toward 0 (center)
        float t = Random.value;
        float distance = Mathf.Pow(t, centerpushPower * 0.3f) * islandHeart.IslandRadius - reduceRadius;

        float x = Mathf.Cos(angle) * distance;
        float z = Mathf.Sin(angle) * distance;

        return new Vector3(x, tideManager.lowTideY, z);
    }

    public int GetRandomItemIndex()
    {
        bool _abletospawn = false;
        int random_spawn = 0;
        while (!_abletospawn)
        {
            random_spawn = Random.Range(0, spawnableItems.Length); 
            if(spawnableItems[random_spawn].ableToSpawn)
                _abletospawn = true;
        }
        return random_spawn;
    }
    
    public int GetRandomRareItemIndex()
    {
        bool _abletospawn = false;
        int random_spawn = 0;
        while (!_abletospawn)
        {
            random_spawn = Random.Range(0, spawnableRareItems.Length); 
            if(spawnableRareItems[random_spawn].ableToSpawn)
                _abletospawn = true;
        }
        return random_spawn;
    }
    
    public bool CalculateIfItemSpawns(int itemIndex)
    {
        spawnItem _spawnItem = spawnableItems[itemIndex];
        float randomNum = Random.Range(0f, 1f);
        bool result = false;
        
        if(randomNum < _spawnItem.probability)
        {
            result = true;
        }
        else
        {
            result = false;
        }
        return result;
    }
}
