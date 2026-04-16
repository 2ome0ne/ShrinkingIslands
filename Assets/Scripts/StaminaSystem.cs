using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
public class StaminaSystem : NetworkBehaviour
{
    [Header("Stamina")] 
    public float CurrentStamina;
    public float MaxStamina;
    public float StaminaRegen;
    
    public float SprintMultiplier;

    [SerializeField] private float maxCanRegen = 1f;
    [SerializeField] private float currentCanRegen;
    [SerializeField] private ParticleSystem sprintParticles;
    
    public bool Sprinting = false;
    [Header("References")]
    [SerializeField] private Slider StaminaSlider;
    [SerializeField] private CharecterController _controller;
    [SerializeField] private PlayerAbillites _playerAbillites;

    void Start()
    {
        CurrentStamina = MaxStamina;
    }

    private bool PlayingParticles;
    private bool Stopped;
    void Update()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if(CurrentStamina > 0)
                Sprinting = true;
            Stopped = false;
            AddSprint();
        }
        if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            if (Sprinting && !Stopped)
            {
                Stopped = true;
                Sprinting = false;
                AddSprint();
                currentCanRegen = maxCanRegen;
            }
        }
        currentCanRegen -= Time.deltaTime;
        if (CurrentStamina > MaxStamina)
        {
            CurrentStamina = MaxStamina;
        }

        if (Sprinting == true)
        {
            if (!PlayingParticles)
            {
                PlayingParticles = true;
                sprintParticles.Play();
            }
            CurrentStamina -= Time.deltaTime;
        }
        else
        {
            if (PlayingParticles)
            {
                PlayingParticles = false;
                sprintParticles.Stop();
            }
        }
        StaminaSlider.value = CurrentStamina;
        StaminaSlider.maxValue = MaxStamina;

        if (CurrentStamina < MaxStamina &&!_playerAbillites.Blocking && currentCanRegen <= 0)
        {
            CurrentStamina += StaminaRegen * Time.deltaTime;
        }
        else if(_playerAbillites.Blocking)
        {
            CurrentStamina -= StaminaRegen * Time.deltaTime;
        }

        if (CurrentStamina <= 0 && Sprinting)
        {
            if(Stopped) return;
            Sprinting = false;
            Stopped = true;
            AddSprint();
        }
    }

    public void EatStamina(float amount)
    {
        CurrentStamina -= amount;
        UpdateStaminaForALlRpc(CurrentStamina);
    }

    public void AddStamina(float amount)
    {
        CurrentStamina += amount;
        UpdateStaminaForALlRpc(CurrentStamina);
    }

    [Rpc(SendTo.Everyone , InvokePermission = RpcInvokePermission.Everyone)]
    private void UpdateStaminaForALlRpc(float currentAmount)
    {
        CurrentStamina = currentAmount;
    }
    
    void AddSprint()
    {
        if (Sprinting == true)
        {
            _controller.SpeedMultiplier += SprintMultiplier;
        }
        else
        {
            _controller.SpeedMultiplier -= SprintMultiplier;
        }
    }
    
}
