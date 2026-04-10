
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class TheSea : NetworkBehaviour
{
    [SerializeField] LayerMask playerLayer;
    [SerializeField] private GameObject WaterPrefab;
    [SerializeField] private Transform checkspawn;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private MeshRenderer terrainRender;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private float yAxis = 3;
    [SerializeField] private float forwardSpeed = 100;

    private void OnTriggerStay(Collider other)
    {
        if ((playerLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            ulong playerId = other.GetComponent<ThePlayerData>().PlayerId.Value;
            gameManager.PlayerDamageServerRpc(other.transform.position , playerId , other.GetComponent<NetworkObject>() , true);
        }
    }

    [SerializeField] private float MinSpawnTime;
    [SerializeField] private float MaxSpawnTime;
    private float currentSpawnTime = 5;
    public override void OnNetworkSpawn()
    {
        StartCoroutine(UpdateSpawnWater());
    }


    IEnumerator UpdateSpawnWater()
    {
        currentSpawnTime = UnityEngine.Random.Range(MinSpawnTime,MaxSpawnTime);
        yield return new WaitForSeconds(currentSpawnTime);
        CalculateWater();
        StartCoroutine(UpdateSpawnWater());
    }

    private void CalculateWater()
    {
        //E
        checkspawn.position = Vector3.zero + Vector3.up * yAxis;
        Quaternion currentrot = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
        checkspawn.rotation = currentrot;
        checkspawn.position = checkspawn.position + checkspawn.forward * forwardSpeed;
        Vector3 posPos = Vector3.zero;
        if (Physics.Raycast(checkspawn.position, -checkspawn.forward, out RaycastHit hit, 100, groundLayer))
        {
            posPos = hit.point;
        }
        else
        {
            Debug.LogWarning("Null Raycast");
        }
        SpawnRpc(posPos);
    }

    [Rpc(SendTo.Everyone)]
    private void SpawnRpc(Vector3 pos)
    {
        Instantiate(WaterPrefab, pos, Quaternion.identity);
    }
}
