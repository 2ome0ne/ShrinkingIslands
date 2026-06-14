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
    private bool throwing = false;

    private bool seeForge;
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
        netObj.Spawn(true);
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
        seeForge = false;
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


        if (Input.GetKey(KeyCode.Q) && !Input.GetMouseButton(1) && HasItem.Value && !seeForge)
        {
            
            if (!throwing)
            {
                throwing = true;
                ThrowAnimatorArmRpc(true);
            }
            if (!forgeInteractor.LookingAtForge)
            {
                CalculateThrowForce();
                uImanager.EnableDisableThrowForceSlider(true);
            }
            else
            {
                seeForge = true;
                Debug.Log("Send Put In forge");
                PutInForgeServerRpc(CurrentHoldObject.GetComponent<NetworkObject>());
            }
        }

        if (Input.GetKeyUp(KeyCode.Q) && !Input.GetMouseButton(1) && !seeForge)
        {
            ThrowAnimatorArmRpc(false);
            GameManager.Instance.soundManager.SpawnSoundRpc(transform.position , 3f , 0.58f , 0.78f , 4);
            throwing = false;
            DropItem(ThrowForce.Value);
            SetThrowForceToZeroServerRpc();
            //uImanager.EnableDisableThrowForceSlider()
            EditThrowForceServerRpc(MinThrowForce);
            animationManager.TriggerThrow();
            _abillites.PunchCooldown = _abillites.MaxPunchCooldown;
            uImanager.EnableDisableThrowForceSlider(false);
        }
    }

    [Rpc(SendTo.Server , InvokePermission = RpcInvokePermission.Everyone)]
    private void PutInForgeServerRpc(NetworkObjectReference NetObj)
    {
        NetObj.TryGet(out NetworkObject _currentHoldObject);
        Debug.Log("Name Of Object Is = " + _currentHoldObject.name);
        if (_currentHoldObject == null) return;
        if(placedForge) return;
        Debug.Log("Looking if it works");
        forgeInteractor.lookingForge.GetComponent<Forge>().PutInForgeRpc(_currentHoldObject);
        placedForge = true;
        DePick();
    }
    

    [Rpc(SendTo.Server , InvokePermission = RpcInvokePermission.Everyone)]
    private void EditThrowForceServerRpc(float value)
    {
        ThrowForce.Value = value;
    }

    [Rpc(SendTo.Everyone , InvokePermission = RpcInvokePermission.Everyone)]
    private void ThrowAnimatorArmRpc(bool value)
    {
        ArmAnimaton.Animator.SetBool("Throwing", value);
    }

    [Rpc(SendTo.Server , InvokePermission = RpcInvokePermission.Everyone)]
    private void SetHasItemServerRpc(bool value)
    {
        HasItem.Value = value;
    }

    [Rpc(SendTo.Server , InvokePermission = RpcInvokePermission.Everyone)]
    private void EditAddThrowForceServerRpc(float value)
    {
        ThrowForce.Value += value;
    }

    [Rpc(SendTo.Server , InvokePermission = RpcInvokePermission.Everyone)]
    private void SetThrowForceToZeroServerRpc()
    {
        ThrowForce.Value = 0;
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
    
    [Rpc(SendTo.Server , InvokePermission = RpcInvokePermission.Everyone)]
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

    [Rpc(SendTo.Server , InvokePermission = RpcInvokePermission.Everyone)]
    private void ThrowForceServerRpc(float throwforce , NetworkObjectReference objRef)
    {
        objRef.TryGet(out NetworkObject netObj);
        netObj.GetComponent<Rigidbody>().AddForce(Cam.forward * throwforce , ForceMode.Impulse);
    }
    
}
