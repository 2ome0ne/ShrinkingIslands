
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ObjectPooler : NetworkBehaviour
{
    public static ObjectPooler instance;

    [System.Serializable]
    public class Pool
    {
        //KEY!!
        public string tag;
        public GameObject prefab;
        public int poolSize;
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictonary = new Dictionary<string, Queue<GameObject>>();
    public Queue<GameObject> objectPool = new Queue<GameObject>();

    private void Awake()
    {
        instance = this;
    }


    public override void OnNetworkSpawn()
    {
        SpawnPoolServerRpc();
    }

    [ServerRpc]
    private void SpawnPoolServerRpc()
    {
        poolDictonary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            for (int i = 0; i < pool.poolSize; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.GetComponent<NetworkObject>().Spawn();
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            
            poolDictonary.Add(pool.tag, objectPool);
        }
    }

    private GameObject objectToSpawn;
    
    public GameObject GetObjectFromPool(string tag)
    {
        GetObjectFromPoolServerRpc(tag);
        return objectToSpawn;
    }

    [ServerRpc]
    public void GetObjectFromPoolServerRpc(string tag)
    {
        if (!poolDictonary.ContainsKey(tag))
        {
            Debug.LogWarning("pool with tag " + tag + " doesn't exist");
        }
        
        objectToSpawn = poolDictonary[tag].Dequeue();
        objectToSpawn.SetActive(true);
    }

    public void ReturnObjectToPool(string tag, NetworkObject obj)
    {
        ReturnObjectToPoolServerRpc(tag, obj);
    }

    [ServerRpc]
    private void ReturnObjectToPoolServerRpc(string tag, NetworkObjectReference RefObj)
    {
        RefObj.TryGet(out NetworkObject netobj);
        if (poolDictonary.TryGetValue(tag, out Queue<GameObject> objectPool))
        {
            objectPool.Enqueue(netobj.gameObject);
        }
        netobj.gameObject.SetActive(false);
        poolDictonary.Add(tag, objectPool);
    }
}
