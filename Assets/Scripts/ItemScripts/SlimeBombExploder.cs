using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class SlimeBombExploder : NetworkBehaviour
{
    [Header("--References--")] [SerializeField]
    private bool ExplodeOnContact;
    private Rigidbody rb;
    [SerializeField] private string ItemIndex;
    [SerializeField] private float HitBoxRange;
    [SerializeField] private GameObject SlimeBlobPrefab;
    [SerializeField] private LayerMask HittableLayer;
    private FollowTransform followTransform;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        followTransform = GetComponent<FollowTransform>();
    }

    private void Update()
    {
        if (!rb.isKinematic && !ExplodeOnContact)
        {
            ExplodeOnContact = true;
        }
    }
    

    private void FixedUpdate()
    {

        Collider[] hits = Physics.OverlapSphere(transform.position, HitBoxRange, HittableLayer);
        if (hits.Length > 0)
        {
            Debug.Log(hits[0].gameObject.name);
            if(!IsServer) return;
            if(!ExplodeOnContact) return;
            if(hits[0].transform == followTransform.player) return;
            SpawnPrefabsServerRpc();
        }
    }

    [ServerRpc]
    private void SpawnPrefabsServerRpc()
    {
        NetworkObject BlobnetObj = Instantiate(SlimeBlobPrefab, transform.position, Quaternion.identity).GetComponent<NetworkObject>();
        BlobnetObj.Spawn();
        ObjectPooler.instance.ReturnObjectToPool(ItemIndex , NetworkObject);
        this.NetworkObject.Despawn(true);
    }

    [ClientRpc]
    private void TestDisablingObjectClientRpc()
    {
        this.gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position , HitBoxRange);
    }
}
