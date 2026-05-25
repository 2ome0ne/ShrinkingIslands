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
    [SerializeField] private Sprite PunchCooldownSprite;
    public float MaxPunchCooldown;
    public float PunchCooldown;
    [SerializeField] private float AttackRange = 2f;
    [SerializeField] private GameObject BoostJumpEffect;
    [SerializeField] private Transform groundPoint;
    [SerializeField] private GameObject hitEffect;
    [Header("dashing")]
    [SerializeField] private float MaxDashCooldown;
    [SerializeField] private float DashStaminaNeeded;
    [SerializeField] private float DashCooldown;
    [SerializeField] private float DashTime = 0.6f;
    [SerializeField] private float DashPower;
    [SerializeField] private GameObject DashEffect;
    [Header("Boost Jumping")] 
    public float boostJumpEyeLevel = -50f;


    [Header("Parrying")]
    public bool Parrying;
    [SerializeField] private GameObject ParryEffect;
    [SerializeField] private float ParryTime;
    [SerializeField] private float currentParry;
    [SerializeField] private float MaxParryStunTime;
    [SerializeField] private float ParryStunTime;
    [SerializeField] private Sprite ParryCooldownSprite;
    public bool succesfulParry;
    [SerializeField] private bool CanParry;
    [Header("--[ Refrences ]--")]
    //[SerializeField] private GameObject HitEffect;
    //Punching
    [SerializeField] private PlayerIconShower playerIconShower;
    [SerializeField] private LayerMask AttackableLayer;
    public LayerMask PickUpLayer;
    public LayerMask PickAbleGearLayer;
    public Transform AttackPoint;
    [SerializeField] private Animator RigAnimator;
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private NetworkAnimator Armanimator;
    [SerializeField] private NetworkAnimator LeftHandAnimator;
    [SerializeField] private PlayerAnimationManager _animationManager;
    [SerializeField] private CharecterController controller;
    private PickUpSystem pickUpSystem;
    public StaminaSystem _staminaSystem;
    
    private bool CanPunch;
    private bool CanDash;
    
    private bool CanBlock = true;
    public bool Blocking;
    public bool currentlyDashing = false;

    public GameObject ParriedObject;
    public float ParryKnockback;
    
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

        ParryUpdate();
    }

    void ParryUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F) && CanParry)
        {
            CanParry = false;
            playerIconShower.AddIcon(MaxParryStunTime ,ParryCooldownSprite,"ParryStun" , true);
            succesfulParry = false;
            ParriedObject = null;
            currentParry = ParryTime;
            ParryStunTime = MaxParryStunTime;
            Parrying = true;
            Armanimator.SetTrigger("Block");
            LeftHandAnimator.SetTrigger("Block");
        }
        //TEST

        if (Parrying)
        {
            currentParry -= Time.deltaTime;
            if (succesfulParry)
            {
                Armanimator.SetTrigger("StopBlock");
                LeftHandAnimator.SetTrigger("SuccesfulParry");
                ParryRpc();
                if (ParriedObject != null)
                {
                    if (ParriedObject.GetComponent<PlayerKnockbackSystem>())
                    {
                        ParryKbRpc();
                        _staminaSystem.AddStamina(100);
                    }
                    else
                    {
                        _staminaSystem.AddStamina(50);
                        ParriedObject.GetComponent<Rigidbody>().AddForce(_cameraController.Camera.forward * ParryKnockback , ForceMode.Impulse);
                    }
                    
                }
                Parrying = false;
                return;
            }
            
            if (currentParry <= 0)
            {
                Parrying = false;
                Armanimator.SetTrigger("StopBlock");
                if (succesfulParry)
                {
                    if (ParriedObject != null)
                    {
                        ParryKbRpc();
                        _staminaSystem.AddStamina(100);
                    }
                    LeftHandAnimator.SetTrigger("SuccesfulParry");
                    ParryRpc();
                }
                else
                {
                    LeftHandAnimator.SetTrigger("StopBlock");
                }
            }
        }
        
        currentParry -= Time.deltaTime;
        ParryStunTime -= Time.deltaTime;
        if (ParryStunTime <= 0 && !Parrying) CanParry = true;
        
    }
    [Rpc(SendTo.Everyone , InvokePermission = RpcInvokePermission.Everyone)]
    private void ParryKbRpc()
    {
        ParriedObject.GetComponent<PlayerKnockbackSystem>().KnockBack(transform.position , ParryKnockback , gameObject);
    }
    
    [Rpc(SendTo.Everyone , InvokePermission = RpcInvokePermission.Everyone)]
    void ParryRpc()
    {
        Instantiate(ParryEffect , transform.position , Quaternion.identity);
    }

    void DashUpdate()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && CanDash)
        {
            GameManager.Instance.soundManager.SpawnSoundRpc(transform.position , 3f , 0.53f , 0.91f , 3);
            DashCooldown = MaxDashCooldown;
            _animationManager.TriggerDash();
            _staminaSystem.EatStamina(DashStaminaNeeded);
            Vector3 spawnpos = transform.position + transform.forward * 3;
            DashEffectRpc(spawnpos,transform.rotation);
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
        currentlyDashing = true;
        controller.SpeedMultiplier += DashPower;
        yield return new WaitForSeconds(DashTime);
        if (!CanDash)
        {
            if(currentlyDashing)
                controller.SpeedMultiplier -= DashPower;
        }
        currentlyDashing = false;
    }

    [Rpc(SendTo.Everyone)]
    private void DashEffectRpc(Vector3 position, Quaternion rotation)
    {
        Instantiate(DashEffect , position , rotation);
    }

    void PunchUpdate()
    {
        if (Input.GetMouseButtonDown(0) && CanPunch == true)
        {
            PunchCooldown = MaxPunchCooldown;
            GameManager.Instance.soundManager.SpawnSoundRpc(transform.position , 3f , 0.58f , 0.78f , 4);
            if (playerIconShower.FindIconWithId("punchCooldown") == null)
            {
                playerIconShower.AddIcon(PunchCooldown ,PunchCooldownSprite,"punchCooldown" , true);
            }
            else
            {
                playerIconShower.EditIcon(playerIconShower.FindIconWithId("punchCooldown") , PunchCooldown);
            }
            
            //left
            RaycastHit hit;
            _animationManager.TriggerThrow();
            if (_cameraController.canBoostJump)
            {
                if (Physics.Raycast(AttackPoint.position, AttackPoint.forward, out hit, AttackRange, PickUpLayer) && GetComponent<PickUpSystem>().HasItem.Value == false)
                {
                    Armanimator.SetTrigger("PickUpPunch");
                             
                    pickUpSystem.PickUpServerRpc(hit.transform.GetComponent<PickableObject>().ObjectIndex);
                    DestroyPickUpServerRpc(hit.transform.GetComponent<NetworkObject>());
                             
                    Armanimator.SetTrigger("Punch");
                    PunchServerRpc(true);
                }
                else if (Physics.Raycast(AttackPoint.position, AttackPoint.forward, out hit, AttackRange, PickAbleGearLayer) && GetComponent<GearManager>().HasGear == false)
                {
                    GearPickUp gear = hit.collider.GetComponent<GearPickUp>();
                    GearManager gearManager = GetComponent<GearManager>();
                    gearManager.ChangeGearServerRpc(gear.gear);
                    gear.DestroyPickUpServerRpc();
                    Armanimator.SetTrigger("Punch");
                    PunchServerRpc(true);
                }
                else
                {
                    Armanimator.SetTrigger("Punch");
                    PunchServerRpc(true);
                }   
            }
            else
            {
                RaycastHit[] hits =
                    Physics.RaycastAll(AttackPoint.position, AttackPoint.forward, AttackRange, PickUpLayer);
                
                RaycastHit[] Gearhit =
                    Physics.RaycastAll(AttackPoint.position, AttackPoint.forward, AttackRange, PickAbleGearLayer);

                if (GetComponent<PickUpSystem>().HasItem.Value == false && hits.Length > 0)
                {
                    foreach (var Hit in hits)
                    {
                        if (Hit.collider.gameObject == this.gameObject) return;
                        
                        Armanimator.SetTrigger("PickUpPunch");
                             
                        pickUpSystem.PickUpServerRpc(Hit.transform.GetComponent<PickableObject>().ObjectIndex);
                        DestroyPickUpServerRpc(Hit.transform.GetComponent<NetworkObject>());
                             
                        Armanimator.SetTrigger("Punch");
                        PunchServerRpc(false);
                        return;
                    }
                }
                else if (GetComponent<GearManager>().HasGear == false && Gearhit.Length > 0)
                {
                    foreach (var Hit in Gearhit)
                    {
                        if (Hit.collider.gameObject == this.gameObject) return;
                             
                        GearPickUp gear = Hit.collider.GetComponent<GearPickUp>();
                        GearManager gearManager = GetComponent<GearManager>();
                        gearManager.ChangeGearServerRpc(gear.gear);
                        gear.DestroyPickUpServerRpc();
                        
                        Armanimator.SetTrigger("Punch");
                        PunchServerRpc(false);
                        return;
                    }
                }
                else
                {
                    Armanimator.SetTrigger("Punch");
                    PunchServerRpc(false);
                }   
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

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void BoostJumpEffectRpc()
    {
        Instantiate(BoostJumpEffect , groundPoint.position , Quaternion.identity);
    }

    [Rpc(SendTo.Server)]
    void PunchServerRpc(bool CanBoost)
    {
        if (CanBoost)
        {
            RaycastHit hit;
            if (Physics.Raycast(AttackPoint.position, AttackPoint.forward, out hit, AttackRange, AttackableLayer))
            {
                Debug.Log(hit.collider.gameObject.name);
                AddKbToPunchRpc(hit.collider.gameObject.GetComponent<NetworkObject>() , hit.point);
                GameManager.Instance.soundManager.SpawnSoundRpc(transform.position , 3f , 0.55f , 0.88f , 7);
                if (hit.collider.gameObject == this.gameObject)
                {
                    BoostJumpEffectRpc();
                }
            }
        }
        else
        {
            RaycastHit[] hits = Physics.RaycastAll(AttackPoint.position, AttackPoint.forward, AttackRange, AttackableLayer);
            foreach (var hit in hits)
            {
                if (hit.collider.gameObject == this.gameObject)
                {
                    return;
                }
                GameManager.Instance.soundManager.SpawnSoundRpc(transform.position , 3f , 0.55f , 0.88f , 7);
                Debug.Log(hit.collider.gameObject.name);
                AddKbToPunchRpc(hit.collider.gameObject.GetComponent<NetworkObject>() , hit.point);
                return;
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void AddKbToPunchRpc(NetworkObjectReference netObj , Vector3 hitpoint)
    {
        netObj.TryGet(out NetworkObject hit);
        hit.GetComponent<PlayerKnockbackSystem>().KnockBack(transform.position, PunchPower , gameObject);
        Instantiate(hitEffect , hitpoint, Quaternion.identity);
    }
}
