using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using EZCameraShake;

public class PlayerAnimationManager : NetworkBehaviour
{
    [Header("References")]
    public NetworkAnimator animator;

    public bool IsSprinting;
    [SerializeField] private bool Jumped = false;

    public CharecterController controller;
    [SerializeField] private StaminaSystem staminaSystem;
    private void Update()
    {
        IsSprinting = staminaSystem.Sprinting;
        if (!IsOwner) return;
        if (controller.PlayerStates == CharecterController.PlayerState.Moving)
        {
            if (IsSprinting)
            {
                SetBoolForAnimatonRpc("Running", true);
                SetBoolForAnimatonRpc("Walking", false);
            }
            else
            {
                SetBoolForAnimatonRpc("Running", false);
                SetBoolForAnimatonRpc("Walking", true);
            }
            SetBoolForAnimatonRpc("InAir", false);
        }
        else if(controller.PlayerStates == CharecterController.PlayerState.AirBorn)
        {
            SetBoolForAnimatonRpc("InAir", true);
            if (!Jumped)
            {
                Jumped = true;
                animator.SetTrigger("Jump");
            }
        }
        else if (controller.PlayerStates == CharecterController.PlayerState.Idle)
        {
            SetBoolForAnimatonRpc("InAir", false);
            SetBoolForAnimatonRpc("Walking", false);
        }

        if (controller.IsGrounded && Jumped)
        {
            CameraShaker.Instance.ShakeOnce(3.5f, 1f , .15f , 2f);
            Jumped = false;
            SetBoolForAnimatonRpc("InAir", false);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void SetBoolForAnimatonRpc(string name,bool value)
    {
        animator.Animator.SetBool(name, value);
    }

    public void TriggerDash()
    {
        animator.SetTrigger("Dash");
    }

    public void TriggerThrow()
    {
        animator.SetTrigger("ThrowItem");
    }
}
