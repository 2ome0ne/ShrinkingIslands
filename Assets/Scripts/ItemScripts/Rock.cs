using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Rock : NetworkBehaviour
{
    
    public GameObject Effect;
    [SerializeField] private float KbForce = 100;
    [SerializeField] private FollowTransform followTransform;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private TrailRenderer trial;

    [SerializeField] private float collidabletime = 2;
    [SerializeField] private float collidableRange = 2.5f;
    [SerializeField]private bool thrown = false;
    [SerializeField]private bool Canhit = false;
    [SerializeField] private bool IsForTutorial;
    [SerializeField] private float currentCollidableTime = 0;

    private void Start()
    {
        currentCollidableTime = collidabletime;
    }

    private void OnCollisionEnter(Collision other)
    {
        //if(!IsHost) return;
        if (!Canhit) return;
        if (other.gameObject != followTransform.player.gameObject)
        {
            CollisonEffectRpc(transform.position);
        }
    }

    private void Update()
    {
        if (!rb.isKinematic)
        {
            if (thrown == false)
            {
                currentCollidableTime = collidabletime;
                Canhit = true;
                thrown = true;
            }

            if (thrown) trial.enabled = true;
            currentCollidableTime -= Time.deltaTime;
            if (currentCollidableTime <= 0 && !IsForTutorial)
            {
                Canhit = false;
            }
        }
        
        if (!thrown) return;
        if (Canhit && thrown)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, collidableRange);
            foreach (var hit in hits)
            {
                if (hit.GetComponent<PlayerKnockbackSystem>() && hit.gameObject != followTransform.player.gameObject)
                {
                    Debug.Log("HIT" + hit.name);
                    hit.GetComponent<PlayerKnockbackSystem>().KnockBack(transform.position, KbForce , gameObject);
                    Canhit = false;
                    if (IsForTutorial)
                    {
                        //NetworkObject.Despawn(true);
                    }
                }
            }
        }
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, collidableRange);
    }

    [Rpc(SendTo.Everyone)]
    private void CollisonEffectRpc(Vector3 postion)
    {
        Instantiate(Effect, postion, Quaternion.identity);
    }
    
    
    
}
