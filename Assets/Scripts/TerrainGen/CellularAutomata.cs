
using System;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class CellularAutomata : NetworkBehaviour
{
    private TerrainGeneration generator;
    
    public TerrainTile[,] grid;
    
    public TerrainTile[,] Iteratedgrid;
    
    public int Iterations = 0;
    
    void Start()
    {
        generator = GetComponent<TerrainGeneration>();
    }

    public void CalcualteTileType()
    {
        generator = GetComponent<TerrainGeneration>();
        grid = generator.grid;
        for (int i = 0; i < Iterations; i++)
        {
            foreach (var tile in grid)
            {
                if (tile != null)
                {
                    int GroundNeighbours = tile.GiveAmoutOfNeighboursGround();
                    int WaterNeighbours = 8 - GroundNeighbours;
                    if (WaterNeighbours > 4)
                    {
                        tile.terrainType = TerrainTile.TerrainTypes.water;
                    }
                    else
                    {
                        tile.terrainType = TerrainTile.TerrainTypes.ground;
                    }
                }
            }
        }
    }
    
    [ClientRpc]
    public void UpdateTerrainTilesClientRpc()
    {
        generator = GetComponent<TerrainGeneration>();
        grid = generator.grid;
        foreach (var tile in grid)
        {
            if (tile != null)
            {
                if (tile.terrainType == TerrainTile.TerrainTypes.water)
                {
                    tile.gameObject.SetActive(false);
                }
                else
                {
                    tile.gameObject.SetActive(true);
                }
            }
        }
        Debug.Log("atleast this");
        Physics.SyncTransforms();
        generator.CombineIslandMeshClientRpc();
        AddColliderServerRpc();
        generator.CompleteIslandClientRpc();
    }

    [Rpc(SendTo.Everyone)]
    void AddColliderServerRpc()
    {
        this.AddComponent<MeshCollider>();
    }
}
