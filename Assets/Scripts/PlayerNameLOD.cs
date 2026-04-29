using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerNameLOD : NetworkBehaviour
{
    [SerializeField] private float currenTargetDistance;
    [SerializeField] private GameObject PlayerName;
    [SerializeField] private Transform target;
    [SerializeField] private float minDistance = 10;

    public override void OnNetworkSpawn()
    {
        if(IsOwner) return;
        Camera camera = Camera.main;
        target = camera.transform;
    }

    private void FixedUpdate()
    {
        if(IsOwner) return;
        Camera camera = Camera.main;
        if (camera != null)
        {
            target = camera.transform;
            currenTargetDistance = Vector3.Distance(transform.position, target.position);
            if (currenTargetDistance < minDistance)
            {
                PlayerName.SetActive(true);
            }
            else
            {
                PlayerName.SetActive(false);
            }
        }
        else
        {
            PlayerName.SetActive(true);
        }
    }
}
