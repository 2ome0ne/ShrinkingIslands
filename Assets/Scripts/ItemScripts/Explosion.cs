using System;
using Unity.Netcode;
using UnityEngine;

public class Explosion : NetworkBehaviour
{
    [SerializeField] private float ExplosionRadius;
    [SerializeField] private float Force;
    [SerializeField] private LayerMask PlayerLayer;
    public override void OnNetworkSpawn()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, ExplosionRadius, PlayerLayer);
        foreach (var hit in hits)
        {
            hit.GetComponent<PlayerKnockbackSystem>().KnockBack(transform , Force);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position , ExplosionRadius);
    }
}
