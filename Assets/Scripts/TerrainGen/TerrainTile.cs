using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainTile : MonoBehaviour
{
    public Vector2Int GridPositon;
    public List<TerrainTile> neighbors = new List<TerrainTile>();
    public List<TerrainTile> neighborsForCal = new List<TerrainTile>(); 

    [SerializeField] private TileGenerator _tileGenerator;
    public enum TerrainTypes
    {
        ground,
        water
    }
    
    public TerrainTypes terrainType;
    
    public bool hasCalculated = false;

    public void AllowCalculateTileMesh()
    {
        // 1. Safety Check: Make sure the list exists and has data
        if (neighborsForCal == null || neighborsForCal.Count == 0)
        {
            SetAllFaces(true);
            _tileGenerator.CalulateMesh();
            return;
        }

        bool allnull = true;
        foreach (TerrainTile tile in neighborsForCal)
        {
            if (tile != null) { allnull = false; break; }
        }

        if (!allnull)
        {
            // Use neighborsForCal.Count to prevent the OutOfRange exception
            for (int i = 0; i < neighborsForCal.Count; i++)
            {
                TerrainTile neighbor = neighborsForCal[i];

                // If neighbor is null (edge of map) OR neighbor is water/not ground
                if (neighbor == null || neighbor.terrainType != TerrainTypes.ground)
                {
                    SetFaceByIndex(i, true);
                }
                // If neighbor IS ground, but at a different height (cliff face)
                else if (neighbor.transform.position.y != this.transform.position.y && neighbor.transform.position.y < this.transform.position.y)
                {
                    SetFaceByIndex(i, true);
                }
                else 
                {
                    // It's a ground tile at the same height, hide the face
                    SetFaceByIndex(i, false);
                }
            }
        }
        else
        {
            SetAllFaces(true);
        }

        _tileGenerator.CalulateMesh();
    }

// Helper method to keep code clean and avoid repeating North/South logic
    private void SetFaceByIndex(int index, bool allowed)
    {
        if (index == 0) _tileGenerator.allowWest = allowed;
        else if (index == 1) _tileGenerator.allowEast = allowed;
        else if (index == 2) _tileGenerator.allowNorth = allowed;
        else if (index == 3) _tileGenerator.allowSouth = allowed;
    }

    private void SetAllFaces(bool allowed)
    {
        _tileGenerator.allowWest = allowed;
        _tileGenerator.allowEast = allowed;
        _tileGenerator.allowNorth = allowed;
        _tileGenerator.allowSouth = allowed;
    }

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
