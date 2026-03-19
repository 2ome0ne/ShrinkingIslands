using UnityEngine;

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
    
    public void KnockBack(Transform selfPos , float KbForce)
    {
        if (playerAbillites.Blocking)
        {
            playerAbillites._staminaSystem.EatStamina(KbForce / 10f);
            return;
        }
        Direction = (transform.position - selfPos.position).normalized;
        Direction.y = 0.5f;
        impact += Direction * KbForce / mass;
        //knockback
    }

    void Update()
    {
        move();
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
