using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class TestAnimationSync : NetworkBehaviour
{
    [Header("Test")] 
    [SerializeField] private NetworkAnimator animator;

    [SerializeField] private GameObject Parenter;

    private void Update()
    {
        if(!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.E))
        {
            animator.SetTrigger("E");
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Parenter.GetComponent<NetworkObject>().TrySetParent(transform);
        }
        
    }
}
