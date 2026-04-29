using System;
using Unity.Netcode;
using UnityEngine;
using EZCameraShake;
public class Explosion : NetworkBehaviour
{
    [SerializeField] private float ExplosionRadius;
    [SerializeField] private float Force;
    [SerializeField] private LayerMask PlayerLayer;
    public override void OnNetworkSpawn()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, ExplosionRadius, PlayerLayer);
        CameraShaker.Instance.ShakeOnce(10f, 6f, 0.1f, 2f);
        foreach (var hit in hits)
        {
            hit.GetComponent<PlayerKnockbackSystem>().KnockBack(transform.position , Force , null);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position , ExplosionRadius);
    }
}
