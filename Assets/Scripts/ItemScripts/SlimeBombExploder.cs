using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class SlimeBombExploder : NetworkBehaviour
{
    [Header("--References--")] [SerializeField]
    private bool ExplodeOnContact = false;
    [SerializeField] private bool Exploded = false;
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
        if (hits.Length > 0 && !Exploded && ExplodeOnContact)
        {
            Debug.Log(hits[0].gameObject.name);
            if(!IsServer) return;
            if(hits[0].transform == followTransform.player) return;
            Exploded = true;
            SpawnPrefabsServerRpc();
        }
    }

    [ServerRpc]
    private void SpawnPrefabsServerRpc()
    {
        NetworkObject BlobnetObj = Instantiate(SlimeBlobPrefab, transform.position, Quaternion.identity).GetComponent<NetworkObject>();
        BlobnetObj.Spawn();
        Debug.Log("Send");
        ObjectPooler.instance.ReturnObjectToPool(ItemIndex , NetworkObject);
        this.NetworkObject.Despawn();
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
