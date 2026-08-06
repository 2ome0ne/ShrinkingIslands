using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class RockBot : NetworkBehaviour
{
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private Transform Cannon;
    [SerializeField] private Transform ShootPoint;
    [SerializeField] private GameObject CannonBullet;
    [SerializeField] private GameObject CrumbleEffect;
    [SerializeField] private float ShootForce;
    [SerializeField] private float LookatSpeed;
    [SerializeField]
    private float DetectRadius;

    [SerializeField]
    private float Cooldown;
    
    private float currentCooldown;
    private bool EnteredCannon;

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.TryGetComponent(out Rock rock))
        {
            Instantiate(CrumbleEffect , transform.position, Quaternion.identity);
            tutorialManager.RockBotDies();
            NetworkObject.Despawn(true);
        }
    }

    private void Update()
    {
        if (tutorialManager.gotPlayer &&
            Vector3.Distance(tutorialManager.controller.transform.position, transform.position) < DetectRadius)
        {
            if (!EnteredCannon)
            {
                EnteredCannon = true;
                currentCooldown = Cooldown;
            }
            
            currentCooldown -= Time.deltaTime;
            
            Cannon.rotation = Quaternion.Slerp(Cannon.rotation , Quaternion.LookRotation(tutorialManager.controller.transform.position - Cannon.position) , Time.deltaTime * LookatSpeed);
            if (currentCooldown <= 0)
            {
                currentCooldown = Cooldown;
                var badRock = Instantiate(CannonBullet , ShootPoint.position, ShootPoint.rotation);
                badRock.GetComponent<NetworkObject>().Spawn(true);
                soundManager.SpawnSoundRpc(transform.position , 20 , 0.45f , 0.9f , 8);
                badRock.GetComponent<Rigidbody>().AddForce(badRock.transform.forward * ShootForce , ForceMode.Impulse);
            }
        }

        if (Vector3.Distance(tutorialManager.controller.transform.position, transform.position) > DetectRadius)
        {
            EnteredCannon = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, DetectRadius);
    }
}
