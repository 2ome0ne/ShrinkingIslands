using System.Collections;
using Unity.Netcode.Components;
using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine.AdaptivePerformance;

public interface IGearBehavior
{
    void OnUsing();
    void OnStopUsing();

    GearManager Holder { get; set; }
}

public class GearManager : NetworkBehaviour
{
    public enum Gear
    {
        None,
        GumRock,
        FlintLock,
        Harpoon,
        ShieldPotion,
        ShockwaveDevice
    }

    [Header("--Settings--")]
    [SerializeField] private KeyCode UseItemKey = KeyCode.F;
    
    [Header("--Refrences--")] 
    public NetworkAnimator leftArmAnimator;

    [SerializeField] private ForgeInteractor forgeInteractor;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private NetworkObject currentHoldingGear;
    
    [SerializeField] private GameObject[] Gears;
    //0 = GumRock , 1 = FlintLock , 2 = ShockwaveDevice
    public Gear currentGear = Gear.None;

    private int currentGearIndex;
    public bool HasGear;
    
    public Transform Harpoonpoint;

    [Rpc(SendTo.Server , InvokePermission = RpcInvokePermission.Everyone)]
    public void ChangeGearServerRpc(Gear newGear)
    {
        if (HasGear) return;
        currentGear = newGear;
        HasGear = true;
        UpdateHoldingGearServerRpc();
        StopHoldingGearRpc(true);
        leftArmAnimator.SetTrigger("PickUpGear");
    }

    [Rpc(SendTo.Everyone , InvokePermission = RpcInvokePermission.Everyone)]
    private void SetHoldingValueRpc(bool value)
    {
        leftArmAnimator.Animator.SetBool("Holding", value);
    }

    [Rpc(SendTo.Server , InvokePermission = RpcInvokePermission.Everyone)]
    private void UpdateHoldingGearServerRpc()
    {
        switch (currentGear)
        {
            case Gear.GumRock:
                currentGearIndex = 0;
                break;
            case Gear.FlintLock:
                currentGearIndex = 1;
                break;
            case Gear.Harpoon:
                currentGearIndex = 2;
                break;
            case Gear.ShieldPotion:
                currentGearIndex = 3;
                break;
        }
        
        currentHoldingGear = Instantiate(Gears[currentGearIndex]).GetComponent<NetworkObject>();
        currentHoldingGear.Spawn(true);
        currentHoldingGear.GetComponent<FollowTransform>().SetTargetTransform(holdPoint , transform);
        Debug.Log("PickedUpGear");
        Debug.Log(currentHoldingGear.name + currentHoldingGear.GetComponent<IGearBehavior>().Holder);
        SetHoldingValueRpc(true);
        SetParentTransformClientRpc(currentHoldingGear , this.NetworkObject ,currentGear);
    }
    
    [ClientRpc]
    private void SetParentTransformClientRpc(NetworkObjectReference networkObjectReference ,NetworkObjectReference holderRef , Gear gear)
    {
        networkObjectReference.TryGet(out NetworkObject netObj);
        holderRef.TryGet(out NetworkObject Ref);
        currentHoldingGear = netObj;
        currentGear = gear;
        HasGear = true;
        netObj.GetComponent<FollowTransform>().SetTargetTransform(holdPoint , transform);
        Debug.Log("got Ref");
        currentHoldingGear.GetComponent<IGearBehavior>().Holder = Ref.GetComponent<GearManager>();
        Debug.Log(currentHoldingGear.GetComponent<IGearBehavior>().Holder.name);
    }

    void ItemUpdate()
    {
        if (currentHoldingGear == null) return;
        
        //Drop Gear
        if (Input.GetKeyDown(KeyCode.Q) && Input.GetMouseButton(1))
        {
            StartCoroutine(DropGearItem());
        }
        
        //Use item
        if (Input.GetKeyDown(UseItemKey))
        {
            Debug.Log("Use Item");
            currentHoldingGear.GetComponent<IGearBehavior>().OnUsing();
        }
        if (Input.GetKeyUp(UseItemKey))
        {
            currentHoldingGear.GetComponent<IGearBehavior>().OnStopUsing();
        }
    }

    IEnumerator DropGearItem()
    {
        bool forged = false;
        if (forgeInteractor.LookingAtForge)
        {
            forgeInteractor.lookingForge.GetComponent<Forge>().PutInForgeRpc(currentHoldingGear.GetComponent<NetworkObject>());
            forged = true;
            RemoveHoldingGearRpc();
        }

        if (!forged)
        {
            leftArmAnimator.SetTrigger("Drop");
        }
        else
        {
            leftArmAnimator.SetTrigger("DropInstant");
        }
        yield return new WaitForSeconds(1f);
        PutInForgeServerRpc(forged);
    }

    [ServerRpc]
    private void PutInForgeServerRpc(bool forged)
    {
        if (!forged)
        {
            DestoryHoldingGear(true);
        }
        else
        {
            DestoryHoldingGear(false);
        }
    }

    public void DestoryHoldingGear(bool destroy)
    {
        UpdateHoldWhenDestoryedRpc();
        if(destroy)
            DestroyHoldRpc();
        StopHoldingGearRpc(false);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void RemoveHoldingGearRpc()
    {
        currentHoldingGear = null;
    }

    [Rpc(SendTo.Everyone , InvokePermission = RpcInvokePermission.Everyone)]
    private void UpdateHoldWhenDestoryedRpc()
    {
        HasGear = false;
        SetHoldingValueRpc(false);
        currentGear = Gear.None;
        StopHoldingGearRpc(false);
    }
    

    [Rpc(SendTo.Server)]
    private void DestroyHoldRpc()
    {
        currentHoldingGear.Despawn();
    }

    [Rpc(SendTo.Everyone)]
    private void StopHoldingGearRpc(bool value)
    {
        leftArmAnimator.Animator.SetBool("GearIsHolding" , value);
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsOwner) return;
        ItemUpdate();
    }
}
