using System;
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
    [SerializeField] private float checkGroundNearRadius = 0.4f;
    [SerializeField] private float centerpushPower;
    [SerializeField] private float reduceRadius = 5;
    [SerializeField] private float UpPushForce;

    [SerializeField] private float StartSpawnDelayTime = 10f;
    private float currentSpawnTime;

    private void Start()
    {
        currentSpawnTime = StartSpawnDelayTime;
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
        
        int index = GetRandomItemIndex();
        if (CalculateIfItemSpawns(index))
        {
            //spawn item
            if (checkIfRandomPointInWater(out Vector3 spawnPos))
            {
                var island = Instantiate(IslandItemPrefab, spawnPos, Quaternion.identity);
                island.GetComponent<SOIslandItemisland>().SpawnThisObject = spawnableItems[index].prefab;
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
    
    private bool checkIfRandomPointInWater(out Vector3 spawnPos)
    {
        spawnPos = GetRandomPointInCircle();
        if (!Physics.CheckSphere(spawnPos, checkGroundNearRadius, GroundLayer))
        {
            return true;
        }

        return false;
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
