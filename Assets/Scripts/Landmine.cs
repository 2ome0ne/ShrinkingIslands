using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Landmine : NetworkBehaviour
{
    [SerializeField] private float DetectionRange;
    [SerializeField] private LayerMask PlayerLayer;
    [SerializeField] private GameObject Explosion;


    [SerializeField] private float minTimer;
    [SerializeField] private float maxTimer;

    [SerializeField] private float blinkTIme;

    [SerializeField] private GameObject blink_prefab;

    private float currentTimer;
    
    private void FixedUpdate()
    {
        if (!IsOwner) return;
        if (Physics.CheckSphere(transform.position, DetectionRange , PlayerLayer))
        {
            GameObject explosion = Instantiate(Explosion , transform.position , Quaternion.identity);
            explosion.GetComponent<NetworkObject>().Spawn();
            NetworkObject.Despawn(true);
        }
        currentTimer -= Time.deltaTime;
        if (currentTimer <= 0)
        {
            currentTimer = UnityEngine.Random.Range(minTimer, maxTimer);
            blink_prefab.SetActive(true);
            Invoke(nameof(StopBlink) ,blinkTIme);
        }
    }

    private void StopBlink()
    {
        blink_prefab.SetActive(false);
    }
    
    

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, DetectionRange);
    }
}
