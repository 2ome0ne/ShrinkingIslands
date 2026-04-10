using System;
using Unity.Netcode;
using UnityEngine;

public class DestroyItself : MonoBehaviour
{
    [SerializeField] private float DestroyTime = 3f;
    private void Awake()
    {
        if (GetComponent<NetworkObject>())
        {
            GetComponent<NetworkObject>().Despawn();
        }
        else
        {
            Destroy(this.gameObject, DestroyTime);
        }
    }
}
