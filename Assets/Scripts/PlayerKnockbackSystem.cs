using System.Collections;
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
    
    [Rpc(SendTo.Owner)]
    public void GetPulledToPositionRpc(Vector3 targetPosition)
    {
        StartCoroutine(PullRoutine(targetPosition));
    }
    
    private IEnumerator PullRoutine(Vector3 target)
    {
        CharacterController cc = GetComponent<CharacterController>();
        // We loop for a short duration or until close enough
        while (Vector3.Distance(transform.position, target) > 0.5f)
        {
            Vector3 direction = (target - transform.position).normalized;
            float pullSpeed = 20f;
            
            cc.Move(direction * pullSpeed * Time.deltaTime);
        
            yield return null;
        }
    }

    public void AddShield(float ShieldTime)
    {
        IconShower.AddIcon(ShieldTime , shieldSprite ,"Shield" , true);
        EnableShieldValueRpc(true);
        Invoke("RemoveShield", ShieldTime);
    }

    public void RemoveShield()
    {
        EnableShieldValueRpc(false);
    }

    [Rpc(SendTo.Everyone , InvokePermission = RpcInvokePermission.Everyone)]
    private void EnableShieldValueRpc(bool value)
    {
        Shield.SetActive(value);
        HasShield = value;
    }

    public void KnockBack(Vector3 attackpositon , float KbForce , GameObject player)
    {
        if (playerAbillites.Clinging)
            playerAbillites.StopClingingFromKBClientRpc();
        if(HasShield) return;
        if (playerAbillites.Parrying)
        {
            //playerAbillites._staminaSystem.EatStamina(KbForce / 100f);
            if (player != null)
            {
                GiveParriedObjectInfoRpc(player, KbForce);
            }
            playerAbillites.succesfulParry = true;
            return;
        }
        CameraShaker.Instance.ShakeOnce(KbForce / 80f, KbForce / 95f, 0.1f, 2f);
        Direction = (transform.position - attackpositon).normalized;
        Direction.y = 0.2f;
        impact += Direction * KbForce / mass;
        //knockback
    }

    [Rpc(SendTo.Everyone)]
    private void GiveParriedObjectInfoRpc(NetworkObjectReference netObj , float KbForce)
    {
        netObj.TryGet(out NetworkObject player);
        playerAbillites.ParriedObject = player.gameObject;
        playerAbillites.ParryKnockback = KbForce;
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
        GameManager.Instance.soundManager.SpawnSoundRpc(transform.position , 30 , 1 , 1 , 9);
        CameraShaker.Instance.ShakeOnce(8f, 6f, 0.1f, 2f);
        Direction = (transform.up * KbForce);
        impact += Direction * KbForce / mass; 
    }

    public void MushroomKnockback(float KbForce)
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
