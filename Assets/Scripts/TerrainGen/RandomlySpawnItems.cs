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
        //the higher it is the more common it is
    }

    
    [SerializeField] private spawnItem[] spawnableItems;
    [SerializeField] private GameObject SpawnIndicator;
    [Header("--[Settings]--")] 
    [SerializeField] private float minSpawnTime;
    [SerializeField] private float maxSpawnTime;
    [SerializeField] private float UpPushForce;

    private float currentSpawnTime;

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

    private void TrySpawnItem()
    {
        int index = GetRandomItemIndex();
        if (CalculateIfItemSpawns(index))
        {
            //spawn item
            GameObject spawnedObj = Instantiate(spawnableItems[index].prefab, GetRandomPostion() , Quaternion.identity);
            spawnedObj.GetComponent<NetworkObject>().Spawn();
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
        while (!HitTheWater)
        {
            ray = new Ray(new Vector3(Random.Range(spt1.position.x, spt2.position.x), spt1.position.y, Random.Range(spt1.position.z, spt2.position.z)), Vector3.down);
            if(Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject.layer == LayerMask.NameToLayer("Water"))
            {
                HitTheWater = true;
            }
            else
            {
                Debug.Log(hit.collider.gameObject.name +" this not water layer");
                HitTheWater = false;
            }
            Debug.DrawRay(ray.origin, hit.point, Color.red);
            Debug.Log(hit.collider.gameObject.name);
            returnPos = hit.point;
        }
        return returnPos;
    }

    public int GetRandomItemIndex()
    {
        return Random.Range(0, spawnableItems.Length);
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
