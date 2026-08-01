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
    public Prop[] props;
    [SerializeField] private float Ylevel;
    //Ray transform
    [SerializeField] private Transform checkspawnPostion;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private TerrainGeneration terrainGenerator;

    [SerializeField] private Vector3 randomPoint1;
    [SerializeField] private Vector3 randomPoint2;
    
    //Get Small Island
    
    [SerializeField] private GameObject Small_Island;
    [SerializeField] private float MinIslandDistance;
    [SerializeField] private float MaxIslandDistance;

    [SerializeField] private bool Test;
    [SerializeField] private float AddyForSmallIsland;
    
    public static PropSpawner Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        SetRandomSpawnPosition();
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

    [ServerRpc]
    public void SpawnIslandServerRpc(int spawn_Amount)
    {
        for (int i = 0; i < spawn_Amount; i++)
        {
            GameObject Small_Island_prefab;
            if (Test)
            {
                Small_Island_prefab = Instantiate(Small_Island , new Vector3(transform.position.x , transform.position.y + AddyForSmallIsland , transform.position.z), Quaternion.identity);
            }
            else
            {
                Small_Island_prefab = Instantiate(Small_Island , transform.position, Quaternion.identity);
            }
            Small_Island_prefab.GetComponent<NetworkObject>().Spawn(true);
            Small_Island_prefab.transform.rotation = Quaternion.Euler(0, Random.Range(0 , 360), 0);
            Small_Island_prefab.transform.position = Small_Island_prefab.transform.forward * Random.Range(MinIslandDistance, MaxIslandDistance);
            //Small_Island_prefab.GetComponent<SmallIslandTile>().Set_a_GTXRpc(Random.Range(0, 2));
        }
    }

    public void SpawnAllProps()
    {
        terrainGenerator.AllowSpawnManagerToSpawnServerRpc();
    }


}
