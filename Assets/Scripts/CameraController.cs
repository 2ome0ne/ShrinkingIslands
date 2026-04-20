using Unity.Netcode;
using UnityEngine;
using EZCameraShake;

public class CameraController : NetworkBehaviour
{

    [Header("Camera Settings")] 
    public float CameraSensitivity = 350f;
    public bool CanMoveCamera = true;
    [Header("Refrences")] 
    public Transform Camera;
    [SerializeField] private Transform CamHolder;
    [SerializeField] private Transform Ppos;
    [SerializeField] private bool SpectatorCamera;

    [SerializeField] private CharecterController controller;
    //FOR SPECTATOR
    [SerializeField] private CharacterController _movementController;
    [SerializeField] private float spectatorSpeed;
    private StaminaSystem stamina_system;
    private PlayerAbillites player_abillites;

    [Header("Camera Fov Change")] 
    public float currentFOV = 60;

    public bool canBoostJump = false;

    [SerializeField] private float lerpMultiplier = 10;
    
    [SerializeField] private float TargetFOV = 60;
    [SerializeField] private float NormalFov = 65f;
    [SerializeField] private float RunningFov = 75f;
    [SerializeField] private float DashFov = 90f;

    [SerializeField] private float walktime = 1.5f;
    [SerializeField] private float sprinttime = 1.3f;
    private float currentShakeTime = 0;
    private Camera cam;
    private float PersonalxRotation;
    private float mouseY;
    void Start()
    {
        cam = Camera.GetComponent<Camera>();
        stamina_system = gameObject.GetComponent<StaminaSystem>();
        player_abillites = gameObject.GetComponent<PlayerAbillites>();
        //Locking Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (!IsOwner)
        {
            Camera.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!IsOwner) return;
        if(!CanMoveCamera) return;
        float mouseX = Input.GetAxis("Mouse X") * CameraSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * CameraSensitivity * Time.deltaTime;
        if(!SpectatorCamera)
            UpdateState();
        if (SpectatorCamera)
            Camera_Movement();
        PersonalxRotation -= mouseY;
        currentShakeTime -= Time.deltaTime;
        //PoseRotateRpc(PersonalxRotation);
        HeadRotateRpc(PersonalxRotation);
        PersonalxRotation = Mathf.Clamp(PersonalxRotation, -90f, 90f);
        //Can boost jump logic
        if(CamHolder != null && !SpectatorCamera) canBoostJump = CamHolder.localEulerAngles.x > player_abillites.boostJumpEyeLevel;
        CamHolder.localRotation = Quaternion.Euler(PersonalxRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private float VerticalRotation;
    void Camera_Movement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 input = new Vector3(horizontal, 0, vertical);

        if (Input.GetKey(KeyCode.Space))
        {
            input.y = 1;
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            input.y = -1;
        }

        transform.Translate(input * Time.deltaTime * spectatorSpeed);
        //_movementController.Move(move * spectatorSpeed * Time.deltaTime);
    }

    private void UpdateState()
    {
        if (player_abillites.currentlyDashing)
        {
            TargetFOV = DashFov;
        }
        else
        {
            if (controller.PlayerStates == CharecterController.PlayerState.Idle)
            {
                TargetFOV = NormalFov;
            }
            else if (controller.PlayerStates == CharecterController.PlayerState.Moving)
            {
                if (stamina_system.Sprinting)
                {
                    TargetFOV = RunningFov;
                    ShakeCamera(3f, 0.1f, .1f, 0.6f , sprinttime);
                }
                else
                {
                    ShakeCamera(3f, 0.1f, .2f, 0.7f , walktime);
                    TargetFOV = NormalFov;
                }
            }
            else if (controller.PlayerStates == CharecterController.PlayerState.AirBorn)
            {
                if (stamina_system.Sprinting)
                {
                    TargetFOV = RunningFov;
                }
            }
        }
        
        currentFOV = Mathf.Lerp(currentFOV, TargetFOV, Time.deltaTime * lerpMultiplier);
        cam.fieldOfView = currentFOV;
    }
    
    private void ShakeCamera(float magnitude , float roughness , float fadeIn , float fadeOut , float time)
    {
        if (currentShakeTime > 0) return;
        currentShakeTime = time;
        CameraShaker.Instance.ShakeOnce(magnitude, roughness, fadeIn, fadeOut);
    }

    [Rpc(SendTo.Everyone)]
    public void PoseRotateRpc(float xRotation)
    {
        xRotation = -xRotation;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        Ppos.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        CamHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    [Rpc(SendTo.NotMe)]
    public void HeadRotateRpc(float xRotation)
    {
        CamHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
