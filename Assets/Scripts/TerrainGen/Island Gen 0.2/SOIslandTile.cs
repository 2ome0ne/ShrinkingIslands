using System;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;


public class SOIslandTile : NetworkBehaviour
{
    public enum IslandType
    {
        Low,
        Medium,
        Tall
    }

    public IslandType islandType;
    public bool originalIsland = false;
    public bool PropsSpawned = false;
    public IslandHeart islandHeart;
    public Transform islandGTX;
    [SerializeField] private float WarningTime = 2;
    public bool Crumbling = false;
    [SerializeField] private float spawnHight = 1;
    [SerializeField] private float ShakeWarningTime = 2.5f;

    [SerializeField] private float crumbleTime;
    [SerializeField] private float CrumbleMultiplier = 0.5f;
    [SerializeField] private float snapDistance = 0.05f;
    [SerializeField] private float IrrosionDistance = 0.4f;
    public bool Spawned = false;
    [SerializeField] private float ShakeMultiplier = 1.5f;
    [SerializeField] private float MoveAmount = 0.7f;

    [SerializeField] private float TiltSpeed = 3;
    [SerializeField] private Transform spawnPostion;
    [SerializeField] private GameObject[] LowIslands;
    [SerializeField] private GameObject[] NormalIslands;
    [SerializeField] private GameObject[] TallIslands;
    
    [SerializeField]
    private SOIslandPropSpawner _soIslandPropSpawner;

    private float CurrentWarningTime;

    public override void OnNetworkSpawn()
    {
        Debug.Log($"Island Spawned - NetId: {NetworkObjectId}, IsServer: {IsServer}, Position: {transform.position}, Parent: {transform.parent}");
        if(!IsServer) return;
        SetRandomIsland();
        transform.Rotate(0 , Random.Range (0f, 360f), 0f);
    }

    public void CrumbleThisIsland()
    {
        Debug.Log($"Island Crumbling");
        CurrentWarningTime = WarningTime;
        Crumbling = true;
    }
    
    void SetRandomIsland()
    {
        Debug.Log($"Island Set");
        int RandomNum = 0;
        float RandomRange = Random.Range(0f, 360f);
        switch (islandType)
        {
            case IslandType.Low:
                Random.Range(0, LowIslands.Length);
                Debug.Log($"Island Spawned - Small");
                break;
            case IslandType.Medium:
                Random.Range(0, NormalIslands.Length);
                Debug.Log($"Island Spawned - Medium");
                break;
            case IslandType.Tall:
                Random.Range(0, TallIslands.Length);
                Debug.Log($"Island Spawned - Large");
                break;
        }
        spawnIslandSeleceted(RandomNum , RandomRange , islandType);
        _soIslandPropSpawner.SpawnPropsServerRpc();
    }
    
    private void spawnIslandSeleceted(int randomNum , float RandomRange , IslandType _islandType)
    {
        GameObject island = null;
        switch (_islandType)
        {
            case IslandType.Low:
                island = Instantiate(LowIslands[randomNum], spawnPostion.position, Quaternion.identity , transform);
                break;
            case IslandType.Medium:
                island = Instantiate(NormalIslands[randomNum], spawnPostion.position, Quaternion.identity , transform);
                break;
            case IslandType.Tall:
                island = Instantiate(TallIslands[randomNum], spawnPostion.position, Quaternion.identity , transform);
                break;
        }
        var islandNetObj = island.GetComponent<NetworkObject>();
        islandNetObj.Spawn(true);
        islandNetObj.TrySetParent(transform);
        island.transform.Rotate(Vector3.zero, RandomRange);
        setSpawnedIslandGTXRpc(islandNetObj);
        _soIslandPropSpawner.islandSurfaceCollider = island.GetComponent<IslandGTX>().Collider;
    }

    [Rpc(SendTo.Everyone)]
    private void setSpawnedIslandGTXRpc(NetworkObjectReference netObjRef)
    {
        netObjRef.TryGet(out NetworkObject netObj);
        islandGTX = netObj.transform;
    }

    public void AnimateSpawn()
    {
        if (originalIsland)
        {
            islandGTX.position = transform.position;
            Spawned = true;
        }
        else
        {
            if (!Spawned)
            {
                islandGTX.position = Vector3.Lerp(islandGTX.position , transform.position , Time.deltaTime * 10f);  
            
                if (Vector3.Distance(islandGTX.position, transform.position) <= IrrosionDistance)
                {
                    islandGTX.position = transform.position; // snap exactly
                    Spawned = true;
                }
            }
        }
    }

    private float elapsed;

    public void AnimateCrumble()
    {
        Debug.Log("ISLAND crumble");
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / crumbleTime);
        float easedT = t * t; 
        
        islandGTX.position = Vector3.Lerp(islandGTX.position , spawnPostion.position , easedT);  
        islandGTX.Rotate(Vector3.forward * Time.deltaTime * TiltSpeed);
        if (elapsed > crumbleTime)
        {
            if(IsServer)
                islandHeart.IslandCrumble(this);
        }
    }

    public void AnimateShake()
    {
        Debug.Log("ISLAND shake");
        islandGTX.position = islandGTX.position + new Vector3(Mathf.Sin(Time.time * ShakeMultiplier) * MoveAmount, 0, 0);
    }


    private void Update()
    {
        if(!IsServer) return;
        if (Spawned)
        {
            if (Crumbling)
            {
                Debug.Log(this.name + "  is Crumbling");
                CurrentWarningTime -= Time.deltaTime;
                if (CurrentWarningTime <= ShakeWarningTime)
                {
                    if (CurrentWarningTime <= 0)
                    {
                        AnimateCrumble();
                    }
                    else
                    {
                        AnimateShake();
                    }
                }
            }
        }
        else
        {
            AnimateSpawn();
        }
    }
}
