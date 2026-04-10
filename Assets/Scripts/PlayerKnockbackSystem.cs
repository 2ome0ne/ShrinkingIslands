using UnityEngine;
using EZCameraShake;
public class PlayerKnockbackSystem : MonoBehaviour
{
    [Header("Knockback Settings")] 
    [SerializeField] private float mass = 3;
    [SerializeField] private float Drag = 5f;
    [SerializeField] private Vector3 Direction;
    [SerializeField] private float KbMultiplier;

    private Vector3 impact = Vector3.zero;
    
    [Header("References")] 
    [SerializeField] private CharacterController Controller;

    [SerializeField]
    private PlayerAbillites playerAbillites;
    
    public void KnockBack(Vector3 attackpositon , float KbForce)
    {
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
        move();
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
