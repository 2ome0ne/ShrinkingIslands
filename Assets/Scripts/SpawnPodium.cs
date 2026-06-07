using System;
using Unity.Netcode;
using UnityEngine;

public class SpawnPodium : NetworkBehaviour
{
    [SerializeField] private float DespawnProprange;
    public override void OnNetworkSpawn()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, DespawnProprange);
        foreach (var hit in colliders)
        {
            if (hit.gameObject.layer == LayerMask.NameToLayer("PropGround"))
            {
                hit.GetComponent<NetworkObject>().Despawn();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, DespawnProprange);
    }
}
