using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class TerrainGeneration : NetworkBehaviour
{
    [Header("--Terrain Settings--")]
    public int width = 16;
    public int height = 16;

    [Header("--Tile Settings--")] 
    public GameObject tilePrefab;
    public int TileSize;
    
    public TerrainTile[,] grid;
    //public bool GenerateComplete = false;
    
    [SerializeField] private SpawnManager spawnManager;

    public override void OnNetworkSpawn()
    {
        if(!IsServer) return;
        sendGenerateGridRpc();
        //spawnManager.CalulateSpawnPointServerRpc();
    }

    [Rpc(SendTo.Owner)]
    public void sendGenerateGridRpc()
    {
        GenerateGridClientRpc();
    }

    [ClientRpc]
    public void GenerateGridClientRpc()
    {
        grid = new TerrainTile[width, height];
        if(IsServer)
            GenerateTileServerRpc();
    }
    [ServerRpc]
    public void GenerateTileServerRpc()
    {
        grid = new TerrainTile[width, height];
        GetComponent<NoiseGeneration>().GetRandomOffsetServerRpc();
    }

    private void GenerateTerrainGrid()
    {
        grid = new TerrainTile[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TerrainTile tile = Instantiate(tilePrefab , new Vector3(x * TileSize, 0, y * TileSize), Quaternion.identity).GetComponent<TerrainTile>();
                
                tile.GridPositon = new Vector2Int(x, y);
                grid[x, y] = tile;
            }
        }
    }

    [ClientRpc]
    public void CalculateNeighborsClientRpc()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != null)
                {
                    grid[x, y].neighbors = GetNeighbors(x, y);

                }
            }
        }
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != null)
                {
                    grid[x, y].CalculateAmoutOfNeighbours();
                }
            }
        }

        for (int i = 0; i < GetComponent<CellularAutomata>().Iterations; i++)
        {
            GetComponent<CellularAutomata>().CalcualteTileType();
        }
        GetComponent<CellularAutomata>().UpdateTerrainTilesClientRpc();
    }
    
    [SerializeField] private Transform CheckPoint;
    [SerializeField] private LayerMask GroundLayer;
    
    [ClientRpc]
    public void CombineIslandMeshClientRpc()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            if(meshFilters[i].gameObject != this.gameObject)
                meshFilters[i].gameObject.SetActive(false);
        }

        Mesh finalMesh = new Mesh();
        finalMesh.CombineMeshes(combine);
    
        // Add a single MeshFilter and MeshRenderer to the Parent object
        GetComponent<MeshFilter>().mesh = finalMesh;
    }

    [ClientRpc]
    public void CompleteIslandClientRpc()
    {
        if (IsServer)
        {
            SetCenterIslandServerRpc();
        }
        else
        {
            transform.position = Vector3.zero;
        }
        if(IsServer)
            spawnManagerGenerateCompleteServerRpc();
    }

    [ServerRpc]
    private void SetCenterIslandServerRpc()
    {
        MeshRenderer islandRenderer = GetComponent<MeshRenderer>();
        Vector3 centerOfIsland = islandRenderer.bounds.center;
        transform.position -= centerOfIsland;
        transform.position = new Vector3(transform.position.x, 0 , transform.position.z);
        Debug.Log("Complete SetCenter");
        //centerpoint.GetComponent<NetworkObject>().Despawn();
    }
    
    [ServerRpc]
    void spawnManagerGenerateCompleteServerRpc()
    {
        Debug.Log("AllowPlayerToSpawn");
        spawnManager.GenerateComplete = true;
    }

    private List<TerrainTile> GetNeighbors(int x, int y)
    {
        List<TerrainTile> neighbors = new List<TerrainTile>();

        Vector2Int[] directions =
        {
            new Vector2Int(0, 1), //North
            new Vector2Int(0, -1), //South
            new Vector2Int(1, 0), //East
            new Vector2Int(-1, 0), //West
            
            // Diagonals (Corners)
            new Vector2Int(1, 1),   // North-East
            new Vector2Int(-1, 1),  // North-West
            new Vector2Int(1, -1),  // South-East
            new Vector2Int(-1, -1)  // South-West
        };

        foreach (Vector2Int direction in directions)
        {
            int checkX = x + direction.x;
            int checkY = y + direction.y;

            if (checkX >= 0 && checkX < width && checkY >= 0 && checkY < height)
            {
                neighbors.Add(grid[checkX, checkY]);
            }
        }
        
        return neighbors;
    }

}
