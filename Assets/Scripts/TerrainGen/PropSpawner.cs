using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class PropSpawner : NetworkBehaviour
{
    [System.Serializable]
    public class Prop
    {
        public GameObject prefab;
        public int MinSpawn;
        public int MaxSpawn;
        public bool CanSpawn = true;
    }
    [SerializeField] private Prop[] props;
    [SerializeField] private float Ylevel;
    //Ray transform
    [SerializeField]private Transform checkspawnPostion;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private TerrainGeneration terrainGenerator;

    [SerializeField] private Vector3 randomPoint1;
    [SerializeField] private Vector3 randomPoint2;

    private void Awake()
    {
        SetRandomSpawnPosition();
    }

    private void Update()
    {
        //TEST
        if (Input.GetKeyDown(KeyCode.G) && IsHost)
        {
            SpawnProp(props[0]);
        }
    }
    
    public void EnablePropByIndex(int index)
    {
        props[index].CanSpawn = true;
    }
    
    private void SetRandomSpawnPosition()
    {
        //random 1
        int tileSize = terrainGenerator.TileSize;
        int moveSpaces = terrainGenerator.width / 2;
        float movePosition = tileSize * moveSpaces;
        randomPoint1 = new Vector3(movePosition, 0, movePosition);
        randomPoint2 = new Vector3(-movePosition, 0, -movePosition);
    }

    public void SpawnAllProps()
    {
        foreach (var prop in props)
        {
            if(prop.CanSpawn)
                SpawnProp(prop);
        }

        terrainGenerator.AllowSpawnManagerToSpawnServerRpc();
    }


    private void SpawnProp(Prop prop)
    {
        for (int i = 0; i < GetSpawnCountForProp(prop); i++)
        {
            GameObject spawnedProp = Instantiate(prop.prefab, GetRandomPositionForProp(), GetRandomRotation());
            spawnedProp.GetComponent<NetworkObject>().Spawn(true);
        }
    }

    private Quaternion GetRandomRotation()
    {
        return Quaternion.Euler(0, Random.Range(0, 360), 0);
    }
    
    private int GetSpawnCountForProp(Prop prop)
    {
        var spawnCount = UnityEngine.Random.Range(prop.MinSpawn, prop.MaxSpawn);
        return spawnCount;
    }

    private Vector3 GetRandomPositionForProp()
    {
        int MaxIterations = 500;
        Vector3 returnPos = Vector3.zero;
        bool hasReturnPosition = false;
        while (MaxIterations > 0 && !hasReturnPosition)
        {
            checkspawnPostion.position = new Vector3(UnityEngine.Random.Range(randomPoint1.x , randomPoint2.x), Ylevel, UnityEngine.Random.Range(randomPoint1.z , randomPoint2.z));
            if (Physics.Raycast(checkspawnPostion.position, Vector3.down, out RaycastHit hit , 100, groundLayer))
            {
                Debug.Log(hit.collider.gameObject.name + "has hit");
                returnPos = hit.point;
                hasReturnPosition = true;
            }
            MaxIterations--;
        }

        if (MaxIterations <= 0)
        {
            Debug.LogError("Max iterations reached");
        }
        return returnPos;
    }
}
