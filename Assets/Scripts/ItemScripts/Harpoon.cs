using System;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.UI;
public class Harpoon : NetworkBehaviour , IGearBehavior
{
    [SerializeField] private Slider HarpoonSlider;
    
    [SerializeField] private float maxCharge;
    [SerializeField] private float currentCharge;
    [SerializeField] private Transform Hook;
    [SerializeField] private GameObject HookPrefab;
    [SerializeField] private Transform ShootPoint;
    [SerializeField] private LineRenderer line;
    [SerializeField] private GameObject harpoonThrow;
    [SerializeField] private Transform HitPlayer;
    [SerializeField] private NetworkAnimator animator;

    [SerializeField] private float knockBackForce = 140;

    public bool hasHit = false;
    private bool charging = false;
    private bool hasShot = false;
    private bool HasCharged = false;
    public override void OnNetworkSpawn()
    {
        HarpoonSlider.maxValue = maxCharge;
    }

    private void Update()
    {
        if(hasShot) return;
        if (charging)
        {
            currentCharge += Time.deltaTime;
            if (currentCharge >= maxCharge)
            {
                hasShot = true;
                RaycastHit hit;
                cameraPosition = Holder.GetComponent<CameraController>().Camera.transform;
                if (Physics.Raycast(cameraPosition.position, cameraPosition.forward, out hit, 100))
                {
                    line.SetPosition(0, ShootPoint.position);
                    line.SetPosition(1, hit.collider.transform.position);
                }
                CheckHitWhatRpc();
                currentCharge = 0;
            }
        }
        else
        {
            currentCharge = 0;
        }
        HarpoonSlider.value = currentCharge;
    }

    private Transform cameraPosition;

    [Rpc(SendTo.Everyone)]
    private void SetHookActiveRpc(bool value)
    {
        Hook.gameObject.SetActive(value);
    }

    [Rpc(SendTo.Server , InvokePermission = RpcInvokePermission.Everyone)]
    private void SpawnHookVisualServerRpc(Vector3 position, Quaternion rotation)
    {
        GameObject _hook = Instantiate(HookPrefab , position , rotation);
        _hook.GetComponent<NetworkObject>().Spawn(true);
    }
    
    [Rpc(SendTo.Everyone , InvokePermission = RpcInvokePermission.Everyone)]
    private void CheckHitWhatRpc()
    {
        RaycastHit hit;
        cameraPosition = Holder.GetComponent<CameraController>().Camera.transform;
        animator.SetTrigger("HarpoonShoot");
        Holder.leftArmAnimator.SetTrigger("HookLockShoot");
        GameManager.Instance.soundManager.SpawnSoundRpc(transform.position, 2 , 1 , 1 , 8);
        SetHookActiveRpc(false);
        if (Physics.Raycast(cameraPosition.position, cameraPosition.forward, out hit, 100))
        {
            line.SetPosition(0, ShootPoint.position);
            line.SetPosition(1, hit.collider.transform.position);
            if (hit.collider.GetComponent<PlayerKnockbackSystem>())
            {
                hasHit = true;
                Hook.position = hit.point;
                HitPlayer = hit.collider.transform;
                SpawnHookVisualServerRpc(hit.collider.transform.position, hit.collider.transform.rotation);
            }
            else
            {
                Hook.position = hit.point;
                SpawnHookVisualServerRpc(hit.collider.transform.position, hit.collider.transform.rotation);
            }
        }
        else
        {
            Debug.Log("No shot found");
            line.SetPosition(0, ShootPoint.position);
            Vector3 shoot = ShootPoint.position;
            shoot += cameraPosition.forward * 50;
            Hook.position = hit.point;
            line.SetPosition(1, shoot);
        }
    }
    
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void PullRpc(Vector3 position, NetworkObjectReference hitRef)
    {
        if (hitRef.TryGet(out NetworkObject hitObj))
        {
            if (hitObj.TryGetComponent<PlayerKnockbackSystem>(out var knockback))
            {
                knockback.GetPulledToPositionRpc(position);
            }
        }
    }
    
    public void Pull()
    {
        PullRpc(Holder.Harpoonpoint.position , HitPlayer.GetComponent<NetworkObject>());
    }
    private void WaitToDestroy()
    {
        WaitToThrowRpc();
        Holder.DestoryHoldingGear();
    }
    
    [Rpc(SendTo.Server , InvokePermission = RpcInvokePermission.Everyone)]
    private void WaitToThrowRpc()
    {
        Rigidbody rb = Instantiate(harpoonThrow , transform.position , transform.rotation).GetComponent<Rigidbody>();
        rb.GetComponent<NetworkObject>().Spawn();
        rb.AddForce(transform.forward * knockBackForce);
    }

    public void OnUsing()
    {
        charging = true;
    }

    public void OnStopUsing()
    {
        charging = false;
    }

    public GearManager Holder { get; set; }
}
