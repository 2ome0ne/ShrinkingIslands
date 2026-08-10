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
        public bool isPropGround = false;
        public bool CanSpawn = true;
    }
    public Prop[] props;
    [SerializeField] private float Ylevel;
    //Ray transform
    [SerializeField] private Transform checkspawnPostion;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask propGroundSpawnableLayer;
    [SerializeField] private TerrainGeneration terrainGenerator;

    [SerializeField] private Vector3 randomPoint1;
    [SerializeField] private Vector3 randomPoint2;
    
    //Get Small Island
    
    [SerializeField] private GameObject Small_Island;
    [SerializeField] private float MinIslandDistance;
    [SerializeField] private float MaxIslandDistance;

    [SerializeField] private float AddyForSmallIsland;
    
    public static PropSpawner Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        //SetRandomSpawnPosition();
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
        terrainGenerator.AllowSpawnManagerToSpawnServerRpc();
    }


}
