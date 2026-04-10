using Unity.Netcode;
using UnityEngine;

public class FlintLock : NetworkBehaviour , IGearBehavior
{
    [SerializeField] private bool hasShot = false;
    [SerializeField] private LineRenderer lineRenderer;

    [SerializeField] private float knockBackForce = 400;
    [SerializeField] private float ThrowForce = 140;
    [SerializeField] private float AnimationTime = 0.6f;

    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject flintLockthrow;

    private Transform cameraPosition;
    public void OnUsing()
    {
        if (!hasShot)
        {
            //hasShot = true;
            ShootRpc();
        }
    }

    [Rpc(SendTo.Everyone , InvokePermission = RpcInvokePermission.Everyone)]
    private void ShootRpc()
    {
        RaycastHit hit;
        cameraPosition = Holder.GetComponent<PlayerAbillites>().AttackPoint;
        Holder.leftArmAnimator.SetTrigger("FlintLockShoot");
        if (Physics.Raycast(cameraPosition.position, cameraPosition.forward, out hit, 100))
        {
            lineRenderer.SetPosition(0, shootPoint.position);
            lineRenderer.SetPosition(1, hit.point);
            if (hit.collider.GetComponent<PlayerKnockbackSystem>())
            {
                hit.collider.GetComponent<PlayerKnockbackSystem>().KnockBack(hit.point, knockBackForce);
            }

            Invoke(nameof(WaitToDestroy), AnimationTime);
        }
        else
        {
            lineRenderer.SetPosition(0, shootPoint.position);
            lineRenderer.SetPosition(1, shootPoint.forward * 50);

            Invoke(nameof(WaitToDestroy), AnimationTime);
        }
    }

    private void WaitToDestroy()
    {
        WaitToThrowRpc();
        Holder.DestoryHoldingGear();
    }

    [Rpc(SendTo.Server , InvokePermission = RpcInvokePermission.Everyone)]
    private void WaitToThrowRpc()
    {
        Rigidbody rb = Instantiate(flintLockthrow , transform.position , transform.rotation).GetComponent<Rigidbody>();
        rb.GetComponent<NetworkObject>().Spawn();
        rb.AddForce(transform.forward * knockBackForce);
    }

    public void OnStopUsing()
    {

    }

    public GearManager Holder { get; set; }
}
