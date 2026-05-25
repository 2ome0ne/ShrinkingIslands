using Unity.Netcode;using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class ShieldPotion : NetworkBehaviour , IGearBehavior
{
    public GearManager Holder { get; set; }

    [Header("Settings")]
    [SerializeField] private float EatTime;
    [SerializeField] private float ShieldTime;
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
            DrinkEveryoneServerRpc();
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
            DrinkUpdateRpc();
            if (currentEatTime <= 0)
            {
                //Use
                DrinkServerRpc();
            }
        }
    }

    [Rpc(SendTo.Everyone , InvokePermission = RpcInvokePermission.Everyone)]
    void DrinkUpdateRpc()
    {
        currentEatTime -= Time.deltaTime;
    }

    [Rpc(SendTo.Server , InvokePermission = RpcInvokePermission.Everyone)]
    public void DrinkServerRpc()
    {
        StopEveryoneServerRpc(true);
        Holder.GetComponent<PlayerKnockbackSystem>().AddShield(ShieldTime);
        Holder.DestoryHoldingGear(true);
    }

    [Rpc(SendTo.Server , InvokePermission = RpcInvokePermission.Everyone)]
    private void DrinkEveryoneServerRpc()
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
