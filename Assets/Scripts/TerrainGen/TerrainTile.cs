using System;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTile : MonoBehaviour
{
    public Vector2Int GridPositon;
    public List<TerrainTile> neighbors = new List<TerrainTile>();
    

    public enum TerrainTypes
    {
        ground,
        water
    }
    
    public TerrainTypes terrainType;
    
    public bool hasCalculated = false;

    public void CalculateAmoutOfNeighbours()
    {
        neighbors.RemoveAll(tile => tile == null);

        hasCalculated = true;
    }

    public int GiveAmoutOfNeighboursGround()
    {
        int amount = 0;
        foreach (TerrainTile tile in neighbors)
        {
            if (tile.terrainType == TerrainTypes.ground)
            {
                amount++;
            }
        }
        
        return amount;
    }

    public void SetHeight(float y)
    {
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }
}
