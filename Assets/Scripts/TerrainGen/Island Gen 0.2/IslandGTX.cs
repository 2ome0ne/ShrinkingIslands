using System;
using UnityEngine;

public class IslandGTX : MonoBehaviour
{
    public Collider Collider;

    public float IslandRadius;

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, IslandRadius);
    }
}
