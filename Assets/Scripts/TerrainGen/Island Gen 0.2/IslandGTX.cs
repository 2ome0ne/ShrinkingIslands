using System;
using Unity.Netcode;
using UnityEngine;

public class IslandGTX : NetworkBehaviour
{
    public Collider Collider;

    public float IslandRadius;
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, IslandRadius);
    }
}
