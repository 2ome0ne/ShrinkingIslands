using UnityEngine;
using EZCameraShake;
using Unity.Netcode;

public class PlayerKnockbackSystem : NetworkBehaviour
{
    [Header("Knockback Settings")] 
    [SerializeField] private float mass = 3;
    [SerializeField] private float Drag = 5f;
    [SerializeField] private Vector3 Direction;
    [SerializeField] private float KbMultiplier;
    [SerializeField] private TheSea sea;

    private Vector3 impact = Vector3.zero;
    public bool HasShield;
    
    [Header("References")] 
    [SerializeField] private CharacterController Controller;
    [SerializeField] private PlayerIconShower IconShower;

    [SerializeField] private GameObject Shield;
    [SerializeField] private Sprite shieldSprite;

    [SerializeField]
    private PlayerAbillites playerAbillites;

    public override void OnNetworkSpawn()
    {
        sea = FindFirstObjectByType<TheSea>();
        AddShield(3);
    }

    public void AddShield(float ShieldTime)
    {
        IconShower.AddIcon(ShieldTime , shieldSprite ,"Shield" , true);
        EnableShieldValueRpc(true);
        HasShield = true;
        Invoke("RemoveShield", ShieldTime);
    }

    public void RemoveShield()
    {
        EnableShieldValueRpc(false);
        HasShield = false;
    }

    [Rpc(SendTo.Everyone , InvokePermission = RpcInvokePermission.Everyone)]
    private void EnableShieldValueRpc(bool value)
    {
        Shield.SetActive(value);
    }

    public void KnockBack(Vector3 attackpositon , float KbForce)
    {
        if(HasShield) return;
        if (playerAbillites.Blocking)
        {
            playerAbillites._staminaSystem.EatStamina(KbForce / 100f);
            return;
        }
        CameraShaker.Instance.ShakeOnce(KbForce / 80f, KbForce / 95f, 0.1f, 2f);
        Direction = (transform.position - attackpositon).normalized;
        Direction.y = 0.2f;
        impact += Direction * KbForce / mass;
        //knockback
    }

    void Update()
    {
        if(!IsOwner) return;
        move();
        if (sea != null && transform.position.y < sea.transform.position.y)
        {
            ulong playerId = gameObject.GetComponent<ThePlayerData>().PlayerId.Value;
            GameManager.Instance.PlayerDamageServerRpc(transform.position , playerId , NetworkObject , true);
        }
    }

    public void SeaKnockback(float KbForce)
    {
        CameraShaker.Instance.ShakeOnce(8f, 6f, 0.1f, 2f);
        Direction = (transform.up * KbForce);
        impact += Direction * KbForce / mass; 
    }

    void move()
    {
        if (impact.magnitude > 0.2f)
        {
            Controller.Move(impact * Time.deltaTime);
        }
        impact = Vector3.Lerp(impact, Vector3.zero, Drag * Time.deltaTime);
    }
}
