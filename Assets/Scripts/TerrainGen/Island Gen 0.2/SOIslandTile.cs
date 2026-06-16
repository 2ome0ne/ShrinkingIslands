using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class SOIslandTile : NetworkBehaviour
{
    public bool originalIsland = false;
    public bool PropsSpawned = false;
    public IslandHeart islandHeart;
    public Transform islandGTX;
    [SerializeField] private float WarningTime = 2;
    [SerializeField] private bool Crumbling = false;
    [SerializeField] private float spawnHight = 1;

    [SerializeField] private float CrumbleMultiplier = 0.5f;
    [SerializeField] private float snapDistance = 0.05f;
    [SerializeField] private float IrrosionDistance = 0.4f;
    [SerializeField] private bool Spawned = false;
    [SerializeField] private float ShakeMultiplier = 1.5f;
    [SerializeField] private float MoveAmount = 0.7f;

    [SerializeField] private float TiltSpeed = 3;
    [SerializeField] private Transform spawnPostion;
    [SerializeField] private GameObject[] Islands;
    
    [SerializeField]
    private SOIslandPropSpawner _soIslandPropSpawner;

    private float CurrentWarningTime;

    public override void OnNetworkSpawn()
    {
        if(!IsServer) return;
        SetRandomIsland();
        transform.Rotate(0 , Random.Range (0f, 360f), 0f);
    }

    public void CrumbleThisIsland()
    {
        CurrentWarningTime = WarningTime;
        Crumbling = true;
    }
    
    void SetRandomIsland()
    {
        int RandomNum = Random.Range(0, Islands.Length);
        float RandomRange = Random.Range(0f, 360f);
        spawnIslandSelecetedRpc(RandomNum , RandomRange);
        _soIslandPropSpawner.SpawnPropsServerRpc();
    }

    [Rpc(SendTo.Server)]
    private void spawnIslandSelecetedRpc(int randomNum , float RandomRange)
    {
        GameObject island = Instantiate(Islands[randomNum], spawnPostion.position, Quaternion.identity , transform);
        var islandNetObj = island.GetComponent<NetworkObject>();
        islandNetObj.Spawn(true);
        islandNetObj.TrySetParent(transform);
        island.transform.Rotate(Vector3.zero, RandomRange);
        islandGTX = island.transform;
        _soIslandPropSpawner.islandSurfaceCollider = island.GetComponent<IslandGTX>().Collider;
    }

    public void AnimateSpawn()
    {
        if (originalIsland)
        {
            islandGTX.position = transform.position;
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

    public void AnimateCrumble()
    {
        islandGTX.position = Vector3.Lerp(islandGTX.position , spawnPostion.position , Time.deltaTime * CrumbleMultiplier);  
        islandGTX.Rotate(Vector3.forward * Time.deltaTime * TiltSpeed);
        if (Vector3.Distance(islandGTX.position, spawnPostion.position) <= snapDistance * 2)
        {
            if(IsServer)
                islandHeart.IslandCrumble(this);
        }
    }

    public void AnimateShake()
    {
        transform.position = transform.position + new Vector3(Mathf.Sin(Time.time * ShakeMultiplier) * MoveAmount, 0, 0);
    }


    private void Update()
    {
        if(!IsOwner) return;
        AnimateSpawn();
        if (Spawned)
        {
            if (Crumbling)
            {
                CurrentWarningTime -= Time.deltaTime;
                if (CurrentWarningTime <= 1.5f && PropsSpawned)
                {
                    AnimateShake();
                    if (CurrentWarningTime <= 0)
                    {
                        AnimateCrumble();
                    }
                }
            }
        }
    }
}
