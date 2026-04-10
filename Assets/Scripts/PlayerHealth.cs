using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using EZCameraShake;
public class PlayerHealth : NetworkBehaviour
{
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(0);
    public List<GameObject> healthOrbs = new List<GameObject>();
    [SerializeField] private GameObject healthOrbPrefab;
    [SerializeField] private GameObject HealthUI;
    [SerializeField] private Transform Content;


    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            HealthUI.SetActive(false);
            return;
        }
        currentHealth.Value = 3;
        UpdateHealthRpc(currentHealth.Value);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void TakeDamageServerRpc()
    {
        currentHealth.Value--;
        UpdateHealthRpc(currentHealth.Value);
    }

    [Rpc(SendTo.Owner)]
    private void UpdateHealthRpc(int health)
    {
        Debug.Log(health);
        
        foreach (GameObject healthorb in healthOrbs)
        {
            Destroy(healthorb);
        }
        healthOrbs.Clear();
        for (int i = 0; i < health; i++)
        {
            GameObject orb = Instantiate(healthOrbPrefab, Content);
            healthOrbs.Add(orb);
        }
    }
}
