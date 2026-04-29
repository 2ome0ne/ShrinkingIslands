using System;
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
    public float headCheckRange = 0.1f;
    public float SpeedMultiplier;
    public float Gravity = -10;
    public float JumpForce = 5;
    public float AirMultiplier = 0f;
    public PlayerState PlayerStates;
    
    public bool CanMove;
    public bool IsGrounded;
    [Header("References")]
    //gravity stuff
    [SerializeField] private Transform headCheck;
    [SerializeField] private Transform GroundCheck;
    [SerializeField] private float GroundCheckRadius;
    [SerializeField] private LayerMask GroundLayer;
    
    [SerializeField] private float currentStep;
    [SerializeField] private float maxStep;
    [SerializeField] private StaminaSystem staminaSystem;
    
    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 move;

    private bool AirMultiply;
    
    

    void Start()
    {
        if(!IsOwner) return;
        controller = transform.GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        if(!IsOwner) return;
        GameManager.Instance.SentEscapePlayer(this.gameObject);
    }

    void Update()
    {
        if(!IsOwner) return;
        if (CanMove)
        {
            Movement();
        }
        //ground Check
        if (Physics.CheckSphere(GroundCheck.position, GroundCheckRadius, GroundLayer))
        {
            if (!IsGrounded)
            {
                GameManager.Instance.soundManager.SpawnSoundRpc(transform.position , 3f , 1f , 0.78f , 6);
            }
            IsGrounded = true;
        }
        else
        {
            IsGrounded = false;
        }
        //IsGrounded = Physics.CheckSphere(GroundCheck.position, GroundCheckRadius, GroundLayer);
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

        if (PlayerStates == PlayerState.Moving)
        {
            currentStep -= Time.deltaTime;
            if (!staminaSystem.Sprinting)
            {
                if (currentStep <= 0)
                {
                    currentStep = maxStep;
                    int randomNum = UnityEngine.Random.Range(0, 2);
                    if (randomNum == 0)
                    {
                        GameManager.Instance.soundManager.SpawnSoundRpc(transform.position , 3f , 0.5f , 0.88f , 1);
                    }
                    else if (randomNum == 1)
                    {
                        GameManager.Instance.soundManager.SpawnSoundRpc(transform.position , 3f , 0.5f , 0.88f , 2);
                    }
                    else
                    {
                        GameManager.Instance.soundManager.SpawnSoundRpc(transform.position , 3f , 0.5f , 0.88f , 3);
                    }
                }
            }
            else
            {
                if (currentStep <= 0)
                {
                    currentStep = maxStep / 2;
                    int randomNum = UnityEngine.Random.Range(0, 2);
                    if (randomNum == 0)
                    {
                        GameManager.Instance.soundManager.SpawnSoundRpc(transform.position , 3f , 0.5f , 0.88f , 1);
                    }
                    else if (randomNum == 1)
                    {
                        GameManager.Instance.soundManager.SpawnSoundRpc(transform.position , 3f , 0.5f , 0.88f , 2);
                    }
                    else
                    {
                        GameManager.Instance.soundManager.SpawnSoundRpc(transform.position , 3f , 0.5f , 0.88f , 3);
                    }
                }
            }
        }
    }

    void GravityUpdate()
    {
        velocity.y += Gravity * Time.deltaTime;
        
        controller.Move(velocity * Time.deltaTime);

        if (Physics.CheckSphere(headCheck.position, headCheckRange, GroundLayer))
        {
            velocity.y = 0;
        }

        if (Input.GetButtonDown("Jump") && IsGrounded)
        {
            velocity.y += Mathf.Sqrt(JumpForce * -2f * Gravity);
            GameManager.Instance.soundManager.SpawnSoundRpc(transform.position , 3f , 0.7f , 0.88f , 5);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(headCheck.position, headCheckRange);
    }
}
