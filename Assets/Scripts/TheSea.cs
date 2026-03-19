
using System;
using Unity.Netcode;
using UnityEngine;

public class TheSea : MonoBehaviour
{
    [SerializeField] LayerMask playerLayer;
    [SerializeField] private GameManager gameManager;

    private void OnTriggerEnter(Collider other)
    {
        if ((playerLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            ulong playerId = other.GetComponent<ThePlayerData>().PlayerId.Value;
            gameManager.PlayerDiesServerRpc(other.transform.position , playerId , other.GetComponent<NetworkObject>());
        }
    }
}
