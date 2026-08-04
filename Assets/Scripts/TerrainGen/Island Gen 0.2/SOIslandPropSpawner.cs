using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SOIslandPropSpawner : NetworkBehaviour
{
    [Header("Spawner References")] 
    public Collider islandSurfaceCollider;
    [SerializeField] private int maxAttemptsPerProp = 150;
    [SerializeField] private float raycastHeight = 15;
    [SerializeField] private LayerMask islandLayerMask;
    [SerializeField] private float maxSlopeAngle = 10;
    [SerializeField] private float minPropSpacing = 2;

    [SerializeField] private List<Vector3> placedPositions;

    [Header("Prop Settings")]
    [SerializeField] private SOIslandTile _soIslandTile;
    [SerializeField] private int min_propCount = 2;
    [SerializeField] private int max_propCount = 5;

    [SerializeField] private GameObject PlaceHolderProp;
    
    [ServerRpc]
    public void SpawnPropsServerRpc()
    {
        foreach (var prop in PropSpawner.Instance.props)
        {
            Debug.Log("IOEE " + prop.prefab.name + " is" + prop.CanSpawn);
            if(prop.CanSpawn)
            {
                int randomspawn = Random.Range(prop.MinSpawn , prop.MaxSpawn);
                for (int i = 0; i < randomspawn; i++)
                {
                    if (TryFindValidSpawnPoint(out Vector3 point, out Vector3 normal))
                    {
                        GameObject newProp = Instantiate(prop.prefab, point, Quaternion.identity , _soIslandTile.islandGTX);
                        var propNetObj = newProp.GetComponent<NetworkObject>();
                        propNetObj.Spawn(true);
                        propNetObj.TrySetParent(_soIslandTile.islandGTX);
                        newProp.transform.Rotate(Vector3.up, Random.Range(0 , 360) , Space.Self);
                    
                        placedPositions.Add(point);
                    }
                
                }
            }

        }
        propsSpawnedRpc();
    }
    

    [Rpc(SendTo.Everyone)]
    private void propsSpawnedRpc()
    {
        _soIslandTile.PropsSpawned = true;
    }

    private bool TryFindValidSpawnPoint(out Vector3 point, out Vector3 normal)
    {
        Bounds bounds = islandSurfaceCollider.bounds;

        for (int attempt = 0; attempt < maxAttemptsPerProp; attempt++)
        {
            // Random XZ point within the island's bounding box
            float randX = Random.Range(bounds.min.x, bounds.max.x);
            float randZ = Random.Range(bounds.min.z, bounds.max.z);
            Vector3 rayOrigin = new Vector3(randX, bounds.max.y + raycastHeight, randZ);

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastHeight * 2f, islandLayerMask))
            {
                // Check slope
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                if (slopeAngle > maxSlopeAngle)
                    continue;
                //check hit is the same terrain
                if (hit.transform != _soIslandTile.islandGTX.GetComponent<IslandGTX>().Collider.transform)
                {
                    break;
                }

                // Check spacing against already-placed props
                bool tooClose = false;
                foreach (var placed in placedPositions)
                {
                    if (Vector3.Distance(hit.point, placed) < minPropSpacing)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    point = hit.point;
                    normal = hit.normal;
                    return true;
                }
            }
        }
        
        point = Vector3.zero;
        normal = Vector3.up;
        return false;
    }
}
