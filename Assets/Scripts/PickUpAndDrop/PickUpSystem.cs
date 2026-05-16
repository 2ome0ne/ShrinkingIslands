using System;
using System.Net;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;

public class PickUpSystem : NetworkBehaviour
{
    [Header("--[Settings]--")] 
    [SerializeField] private NetworkVariable<float> ThrowForce = new NetworkVariable<float>(0f);
    
    [SerializeField] private float MaxThrowForce;
    [SerializeField] private float MinThrowForce;
    [SerializeField] private float ThrowforceMultiplier = 1000;

    [Header("--[References]--")] 
    private PlayerUImanager uImanager;
    [SerializeField] private ForgeInteractor forgeInteractor;
    [SerializeField] private PlayerAbillites _abillites;
    [SerializeField] private Transform HoldPoint;
    [SerializeField] private Transform Cam;
    [SerializeField] private PlayerAnimationManager animationManager;
    [SerializeField] private NetworkAnimator ArmAnimaton;
    public NetworkVariable<bool> HasItem;

    [SerializeField] private Transform ThrowPoint;
    public Transform CurrentHoldObject;
    [SerializeField] private GameObject[] PlayerObjects;
    private bool placedForge = false;
    //1 TestCube
    //2 SlimeBomb
    
    //Throw stuff

    public override void OnNetworkSpawn()
    {
        uImanager =GetComponent<PlayerUImanager>();
        uImanager.ThrowForceSlider.maxValue = MaxThrowForce;
    }


    [ServerRpc]
    public void PickUpServerRpc(int ItemIndex)
    {
        if (HasItem.Value) return;
        Debug.Log("Pick up server");
        CurrentHoldObject = Instantiate(PlayerObjects[ItemIndex].transform);
        placedForge = false;
        NetworkObject netObj = CurrentHoldObject.GetComponent<NetworkObject>();
        netObj.Spawn();
        SetParentTransformClientRpc(netObj);

        SetHoldingBooleanServerRpc(true);
    }

    [ClientRpc]
    private void SetParentTransformClientRpc(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out NetworkObject netObj);
        netObj.GetComponent<FollowTransform>().SetTargetTransform(this.HoldPoint.transform , this.transform);
        netObj.GetComponent<Collider>().enabled = false;
        netObj.GetComponent<Rigidbody>().isKinematic = true;
        CurrentHoldObject = netObj.transform;
    }
    

    [Rpc(SendTo.Everyone)]
    private void SetHoldingBooleanServerRpc(bool value)
    {
        ArmAnimaton.Animator.SetBool("IsHolding", value);
        placedForge = false;
    }

    private void Update()
    {
        if(!IsOwner) return;
        if (CurrentHoldObject != null)
        {
            SetHasItemServerRpc(true);
        }
        else
        {
            SetHasItemServerRpc(false);
        }


        if (Input.GetKey(KeyCode.Q) && !Input.GetMouseButton(1))
        {
            if (!forgeInteractor.LookingAtForge)
            {
                CalculateThrowForce();
                uImanager.EnableDisableThrowForceSlider(true);
            }
            else
            {
                PutInForgeServerRpc();
            }
        }

        if (Input.GetKeyUp(KeyCode.Q) && !Input.GetMouseButton(1))
        {
            DropItem(ThrowForce.Value);
            EditThrowForceServerRpc(MinThrowForce);
            animationManager.TriggerThrow();
            _abillites.PunchCooldown = _abillites.MaxPunchCooldown;
            uImanager.EnableDisableThrowForceSlider(false);
        }
    }

    [ServerRpc]
    private void PutInForgeServerRpc()
    {
        if (CurrentHoldObject == null) return;
        if(placedForge) return;
        forgeInteractor.lookingForge.GetComponent<Forge>().PutInForgeRpc(CurrentHoldObject.GetComponent<NetworkObject>());
        placedForge = true;
        DePick();
    }

    [ServerRpc]
    private void EditThrowForceServerRpc(float value)
    {
        ThrowForce.Value = value;
    }

    [ServerRpc]
    private void SetHasItemServerRpc(bool value)
    {
        HasItem.Value = value;
    }

    [ServerRpc]
    private void EditAddThrowForceServerRpc(float value)
    {
        ThrowForce.Value += value;
    }

    public void DePick()
    {
        SetHoldingBooleanServerRpc(false);
        SetNullClientRpc();
    }

    private void CalculateThrowForce()
    {
        if (ThrowForce.Value < MaxThrowForce)
        {
            Debug.Log("Charging =" + ThrowForce.Value);
            EditAddThrowForceServerRpc(ThrowforceMultiplier * Time.deltaTime);
        }
        uImanager.SetThrowForceSlider(ThrowForce.Value);
    }

    [ServerRpc]
    public void Destory_Item_Thats_CurrentlyHoldingServerRpc()
    {
        CurrentHoldObject.GetComponent<NetworkObject>().Despawn();
    }
    
    

    [ClientRpc]
    private void SetNullClientRpc()
    {
        this.CurrentHoldObject = null;
    }
    
    private void DropItem(float throwforce)
    {
        if(!HasItem.Value) return;
        SetFollowTransformNullServerRpc(CurrentHoldObject.GetComponent<NetworkObject>() , throwforce);
        SetHoldingBooleanServerRpc(false);
    }
    
    [ServerRpc]
    private void SetFollowTransformNullServerRpc(NetworkObjectReference networkObjectReference , float throwforce)
    {
        networkObjectReference.TryGet(out NetworkObject netObj);
        SetFollowTransformNullClientRpc(netObj , throwforce);
    }

    [ClientRpc]
    private void SetFollowTransformNullClientRpc(NetworkObjectReference networkObjectReference , float throwforce)
    {
        networkObjectReference.TryGet(out NetworkObject netObj);
        netObj.GetComponent<FollowTransform>().SetTargetTransform(null , this.transform);
        netObj.transform.position = ThrowPoint.position;
        netObj.GetComponent<Rigidbody>().isKinematic = false;
        netObj.GetComponent<Rigidbody>().useGravity = true;
        if (netObj.GetComponent<Collider>())
        {
            netObj.GetComponent<Collider>().enabled = true; 
        }

        ThrowForceServerRpc(throwforce, netObj);
        //netObj.GetComponent<Rigidbody>().linearVelocity = Cam.forward * throwforce * Time.deltaTime;
        this.CurrentHoldObject = null;
    }

    [ServerRpc]
    private void ThrowForceServerRpc(float throwforce , NetworkObjectReference objRef)
    {
        objRef.TryGet(out NetworkObject netObj);
        netObj.GetComponent<Rigidbody>().AddForce(Cam.forward * throwforce , ForceMode.Impulse);
    }
    
}
