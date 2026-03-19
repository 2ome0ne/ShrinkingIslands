using Unity.Netcode;
using UnityEngine;

public class CharecterController : NetworkBehaviour
{
    public enum PlayerState
    {
        Idle,
        Moving,
        AirBorn
    }

    [Header("Player Settings")] 
    public float DefaultSpeed = 5;
    public float SpeedMultiplier;
    public float Gravity = -10;
    public float JumpForce = 5;
    public float AirMultiplier = 0.4f;
    public PlayerState PlayerStates;
    
    public bool CanMove;
    public bool IsGrounded;
    [Header("References")]
    //gravity stuff
    [SerializeField] private Transform GroundCheck;
    [SerializeField] private float GroundCheckRadius;
    [SerializeField] private LayerMask GroundLayer;
    
    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 move;

    private bool AirMultiply;

    void Start()
    {
        if(!IsOwner) return;
        controller = transform.GetComponent<CharacterController>();
    }
    
    void Update()
    {
        if(!IsOwner) return;
        if (CanMove)
        {
            Movement();
        }
        //ground Check
        IsGrounded = Physics.CheckSphere(GroundCheck.position, GroundCheckRadius, GroundLayer);
        if (IsGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        GravityUpdate();
        UpdateState();
    }

    void UpdateState()
    {
        if (IsGrounded != true)
        {
            PlayerStates = PlayerState.AirBorn;
        }
        else if (move != Vector3.zero)
        {
            PlayerStates = PlayerState.Moving;
        }
        else
        {
            PlayerStates = PlayerState.Idle;
        }

        if (PlayerStates == PlayerState.AirBorn && AirMultiply == false)
        {
            AirMultiply = true;
            SpeedMultiplier += AirMultiplier;
        }
        else if (PlayerStates != PlayerState.AirBorn && AirMultiply == true)
        {
            AirMultiply = false;
            SpeedMultiplier -= AirMultiplier;
        }
    }

    void Movement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        move = transform.right * horizontal + transform.forward * vertical;
        controller.Move(move * DefaultSpeed * SpeedMultiplier * Time.deltaTime);
    }

    void GravityUpdate()
    {
        velocity.y += Gravity * Time.deltaTime;
        
        controller.Move(velocity * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && IsGrounded)
        {
            velocity.y += Mathf.Sqrt(JumpForce * -2f * Gravity);
        }
    }
}
