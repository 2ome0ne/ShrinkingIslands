using System;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerAbillites : NetworkBehaviour
{
    [Header("--[ Settings ]--")] 
    [Header("punching")]
    [SerializeField] float PunchPower;
    [SerializeField] private float MaxPunchCooldown;
    [SerializeField] private float PunchCooldown;
    [SerializeField] private float AttackRange = 2f;
    [Header("dashing")]
    [SerializeField] private float MaxDashCooldown;
    [SerializeField] private float DashStaminaNeeded;
    [SerializeField] private float DashCooldown;
    [SerializeField] private float DashTime = 0.6f;
    [SerializeField] private float DashPower;
    [Header("blocking")]
    [SerializeField] private float BlockDelay;
    [SerializeField] private float BlockCooldown;
    [SerializeField] private float BlockStunTime;
    [Header("TEST")]
    [SerializeField] private bool Testing = false;
    [Header("--[ Refrences ]--")] 
    //[SerializeField] private GameObject HitEffect;
    //Punching
    [SerializeField] private LayerMask AttackableLayer;
    public LayerMask PickUpLayer;
    public Transform AttackPoint;
    [SerializeField] private Animator RigAnimator;
    [SerializeField] private NetworkAnimator Armanimator;
    [SerializeField] private CharecterController controller;
    private PickUpSystem pickUpSystem;
    public StaminaSystem _staminaSystem;
    
    private bool CanPunch;
    private bool CanDash;
    
    private bool CanBlock = true;
    public bool Blocking;

    private void Awake()
    {
        pickUpSystem = GetComponent<PickUpSystem>();
    }

    void Update()
    {
        if (!IsOwner) return;
        if (!Blocking)
        {
            DashUpdate();
            PunchUpdate();
        }

        BlockUpdate();
    }

    void BlockUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F) && CanBlock)
        {
            Blocking = true;
            Armanimator.SetTrigger("Block");
            controller.SpeedMultiplier -= 0.7f;
        }
        //TEST
        if (Input.GetKeyDown(KeyCode.T) && CanBlock && Testing)
        {
            Blocking = true;
            Armanimator.SetTrigger("Block");
            controller.SpeedMultiplier -= 0.7f;
        }

        if (Input.GetKeyUp(KeyCode.F) && Blocking)
        {
            Blocking = false;
            Armanimator.SetTrigger("StopBlock");
            CanBlock = true;
            controller.SpeedMultiplier += 0.7f;
        }

        if (Blocking == true)
        {
            if (_staminaSystem.CurrentStamina <= 0)
            {
                //block cooldown
                BlockCooldown = BlockStunTime;
                Blocking = false;
                Armanimator.SetTrigger("StopBlock");
                controller.SpeedMultiplier += 0.7f;
            }

            CanBlock = false;
        }
        else if(BlockCooldown < 0)
        {
            CanBlock = true;
        }

        if (BlockCooldown > 0)
        {
            BlockCooldown -= Time.deltaTime;
        }
    }

    void DashUpdate()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && CanDash)
        {
            DashCooldown = MaxDashCooldown;
            _staminaSystem.EatStamina(DashStaminaNeeded);
            StartCoroutine(Dashing());
        }

        if (DashCooldown < 0 && _staminaSystem.CurrentStamina >= DashStaminaNeeded)
        {
            CanDash = true;
        }
        else
        {
            DashCooldown -= Time.deltaTime;
            CanDash = false;
        }
    }

    IEnumerator Dashing()
    {
        controller.SpeedMultiplier += DashPower;
        yield return new WaitForSeconds(DashTime);
        controller.SpeedMultiplier -= DashPower;
    }

    void PunchUpdate()
    {
        if (Input.GetMouseButtonDown(0) && CanPunch == true)
        {
            PunchCooldown = MaxPunchCooldown;
            //left
            RaycastHit hit;
            if (Physics.Raycast(AttackPoint.position, AttackPoint.forward, out hit, AttackRange, PickUpLayer) && GetComponent<PickUpSystem>().HasItem.Value == false)
            {
                Armanimator.SetTrigger("PickUpPunch");
                
                pickUpSystem.PickUpServerRpc(hit.transform.GetComponent<PickableObject>().ObjectIndex);
                DestroyPickUpServerRpc(hit.transform.GetComponent<NetworkObject>());
                
                Armanimator.SetTrigger("Punch");
                PunchServerRpc();
            }
            else
            {
                Armanimator.SetTrigger("Punch");
                PunchServerRpc();
            }
        }

        if (!CanPunch)
        {
            PunchCooldown -= Time.deltaTime;
        }
        else
        {
            Debug.DrawRay(AttackPoint.position , AttackPoint.forward , Color.red , AttackRange);
        }
        if (PunchCooldown < 0)
        {
            CanPunch = true;
        }
        else
        {
            CanPunch = false;
        }
    }

    [ServerRpc]
    public void DestroyPickUpServerRpc(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);
        networkObject.Despawn(true);
    }

    [Rpc(SendTo.Everyone)]
    void PunchServerRpc()
    {
        RaycastHit hit;
        if (Physics.Raycast(AttackPoint.position , AttackPoint.forward, out hit , AttackRange, AttackableLayer))
        {
            Debug.Log(hit.collider.gameObject.name);
            hit.collider.GetComponent<PlayerKnockbackSystem>().KnockBack(transform , PunchPower);
        }
    }
}
