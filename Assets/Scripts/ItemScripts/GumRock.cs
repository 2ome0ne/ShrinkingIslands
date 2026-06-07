using Unity.Netcode;using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class GumRock : NetworkBehaviour , IGearBehavior
{
    public GearManager Holder { get; set; }

    [Header("Settings")]
    [SerializeField] private float EatTime;
    [SerializeField] private float AddStaminaValue;
    [SerializeField] private float currentEatTime;
    [SerializeField] private bool Eating = false;
    [SerializeField] private AudioSource EatingSound;
    public override void OnNetworkSpawn()
    {
        currentEatTime = EatTime;
    }

    public void OnUsing()
    {
        if (!Eating)
        {
            EatEveryoneServerRpc();
            StopEveryoneServerRpc(false);
            Eating = true;
        }
        EatSoundRpc();
    }

    [Rpc(SendTo.Everyone , InvokePermission = RpcInvokePermission.Everyone)]
    private void EatSoundRpc()
    {
        Debug.Log("PLAY Using");
        EatingSound.Play();
    }
    [Rpc(SendTo.Everyone , InvokePermission = RpcInvokePermission.Everyone)]
    private void StopEatSoundRpc()
    {
        EatingSound.Stop();
    }

    public void OnStopUsing()
    {
        if (Eating)
        {
            StopEveryoneServerRpc(true);
            Eating = false;
            Debug.Log("Reset");
            currentEatTime = EatTime;
        }
    }

    void Update()
    {
        //if (!IsOwner) return;
        if (Eating)
        {
            Debug.Log("Eating");
            EatUpdateRpc();
            if (currentEatTime <= 0)
            {
                //Use
                EatServerRpc();
            }
        }
    }

    [Rpc(SendTo.Everyone , InvokePermission = RpcInvokePermission.Everyone)]
    void EatUpdateRpc()
    {
        currentEatTime -= Time.deltaTime;
    }

    [Rpc(SendTo.Server , InvokePermission = RpcInvokePermission.Everyone)]
    public void EatServerRpc()
    {
        StopEveryoneServerRpc(true);
        Holder.GetComponent<StaminaSystem>().AddStamina(AddStaminaValue);
        Holder.DestoryHoldingGear();
    }

    [Rpc(SendTo.Server , InvokePermission = RpcInvokePermission.Everyone)]
    private void EatEveryoneServerRpc()
    {
        Holder.leftArmAnimator.SetTrigger("Eat");
    }
    
    [Rpc(SendTo.Everyone , InvokePermission = RpcInvokePermission.Everyone)]
    private void StopEveryoneServerRpc(bool value)
    {
        StopEatSoundRpc();
        Debug.Log("Stop Using");
        Holder.leftArmAnimator.Animator.SetBool("StopEating" , value);
    }
}
