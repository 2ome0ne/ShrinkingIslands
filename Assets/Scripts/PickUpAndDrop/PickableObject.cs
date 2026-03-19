using System;
using Unity.Netcode;
using UnityEngine;

public class PickableObject : NetworkBehaviour
{
    public int ObjectIndex;
    private FollowTransform followTransform;

    private void Start()
    {
        followTransform = GetComponent<FollowTransform>();
    }
}
