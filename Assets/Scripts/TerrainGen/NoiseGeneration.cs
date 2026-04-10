using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;

public class NoiseGeneration : NetworkBehaviour
{
    private TerrainGeneration generator;

    [SerializeField] private float noiseScale = 0.2f;
    [SerializeField] private float threshold = 0.5f;
    [SerializeField] private float higherThreshold = 0.6f;
    [SerializeField] private float highestThreshold = 0.7f;

    [SerializeField] private float higherSize = 5;
    [SerializeField] private float highestSize = 10;
    [SerializeField] private float falloffStrength = 3;

    public float offsetX;
    public float offsetY;
    
    private void Start()
    {
        generator = GetComponent<TerrainGeneration>();
    }
    [ServerRpc]
    public void GetRandomOffsetServerRpc()
    {
        offsetX = UnityEngine.Random.Range(0 ,100);
        offsetY = UnityEngine.Random.Range(0 ,100);
        GenerateGridClientRpc(offsetX , offsetY);
    }

    private float[,] fallOffMap(int size)
    {
        float[,] map = new float[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float x = i / (float)size * 2 - 1;
                float y = i / (float)size * 2 - 1;
                float Value =Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i , j] = Value;
            }
        }
        return map;
    }
    
    [ClientRpc]
    public void GenerateGridClientRpc(float ofX , float ofY)
    {
        offsetX = ofX;
        offsetY = ofY;
        Debug.Log(offsetX + " x : y " + offsetY);
        generator = GetComponent<TerrainGeneration>();
        float[,] fallofMap = fallOffMap(generator.width);
        for (int x = 0; x < generator.width; x++)
        {
            for (int y = 0; y < generator.height; y++)
            {
                float xCoord = (float)x * noiseScale + offsetX;
                float yCoord = (float)y * noiseScale + offsetY;
                //float noiseValue = Mathf.Clamp01(Mathf.PerlinNoise(xCoord, yCoord) - fallofMap[(int)xCoord, (int)yCoord]);
                float noiseValue = Mathf.PerlinNoise(xCoord, yCoord);
                Vector3 spawnPos = new Vector3(x * generator.TileSize + transform.position.x, 0, y * generator.TileSize + transform.position.z);
                if (noiseValue > threshold)
                {
                    if (noiseValue > higherThreshold)
                    {
                        if (noiseValue > highestThreshold)
                        {
                            TerrainTile tile = Instantiate(generator.tilePrefab, spawnPos, Quaternion.identity , transform).GetComponent<TerrainTile>();
                            tile.SetHeight(highestSize);
                            tile.GridPositon = new Vector2Int(x, y);
                            generator.grid[x, y] = tile;
                        }
                        else
                        {
                            TerrainTile tile = Instantiate(generator.tilePrefab, spawnPos, Quaternion.identity , transform).GetComponent<TerrainTile>();
                            tile.SetHeight(higherSize);
                            tile.GridPositon = new Vector2Int(x, y);
                            generator.grid[x, y] = tile;
                        }
                    }
                    else
                    {
                        TerrainTile tile = Instantiate(generator.tilePrefab, spawnPos, Quaternion.identity , transform).GetComponent<TerrainTile>();
                        tile.GridPositon = new Vector2Int(x, y);
                        generator.grid[x, y] = tile;
                    }
                }
                else
                {
                    TerrainTile tile = Instantiate(generator.tilePrefab, spawnPos, Quaternion.identity , transform).GetComponent<TerrainTile>();
                    tile.terrainType = TerrainTile.TerrainTypes.water;
                    tile.GridPositon = new Vector2Int(x, y);
                    generator.grid[x, y] = tile;
                }
            }
        }

        generator.CalculateNeighborsClientRpc();
    }

}
