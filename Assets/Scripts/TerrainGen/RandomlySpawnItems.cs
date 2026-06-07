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
    [Header("--[Settings]--")] 
    public float minSpawnTime;
    public float maxSpawnTime;
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(ray.origin, ray.direction * 100f);
    }

    [ServerRpc]
    public void EnableItemToSpawnByIndexServerRpc(int index)
    {
        Debug.Log(index + " is enabled");
        spawnableItems[index].ableToSpawn = true;
    }

    private void TrySpawnItem()
    {
        int index = GetRandomItemIndex();
        if (CalculateIfItemSpawns(index))
        {
            //spawn item
            GameObject spawnedObj = Instantiate(spawnableItems[index].prefab, GetRandomPostion() , Quaternion.identity);
            spawnedObj.GetComponent<NetworkObject>().Spawn(true);
            SetNoGravityToItemServerRpc(spawnedObj);
            SpawnTheIndicatorServerRpc(spawnedObj.transform.position);
            //Spawn Push force
            spawnedObj.GetComponent<Rigidbody>().AddForce(UpPushForce * spawnedObj.transform.up, ForceMode.Force);
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

    [SerializeField] private LayerMask WaterLayer;


    private Ray ray;
    private Vector3 GetRandomPostion()
    {
        Vector3 returnPos = new Vector3();
        bool HitTheWater = false;
        int maxcal = 100;
        int currentcal = 0;
        while (!HitTheWater ||currentcal < maxcal)
        {
            ray = new Ray(new Vector3(Random.Range(spt1.position.x, spt2.position.x), spt1.position.y, Random.Range(spt1.position.z, spt2.position.z)), Vector3.down);
            if(Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject.layer == LayerMask.NameToLayer("Water"))
            {
                HitTheWater = true;
            }
            else
            {
                currentcal++;
                HitTheWater = false;
            }

            if (currentcal >= maxcal)
            {
                Debug.LogWarning("To Many Calculations");
            }
            Debug.DrawRay(ray.origin, hit.point, Color.red);
            returnPos = hit.point;
        }
        return returnPos;
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
