using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
public class StaminaSystem : NetworkBehaviour
{
    [Header("Stamina")] 
    public float CurrentStamina;
    public float MaxStamina;
    public float StaminaRegen;
    [SerializeField] private float CantSprintTime = 0.7f;

    public float CurrentSprintTime;
    public float SprintMultiplier;

    [SerializeField] private float maxCanRegen = 1f;
    [SerializeField] private float currentCanRegen;
    [SerializeField] private ParticleSystem sprintParticles;
    
    public bool Sprinting = false;

    [Header("References")] 
    [SerializeField] private Sprite cantSprintIcon;
    [SerializeField] private PlayerIconShower playerIcon;
    [SerializeField] private Slider StaminaSlider;
    [SerializeField] private CharecterController _controller;
    [SerializeField] private PlayerAbillites _playerAbillites;

    void Start()
    {
        CurrentStamina = MaxStamina;
        if(!IsOwner) StaminaSlider.gameObject.SetActive(false);
    }

    private bool PlayingParticles;
    private bool Stopped;
    private bool PressingSprint;
    void Update()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.LeftShift) && CurrentSprintTime <= 0)
        {
            PressingSprint = true;
            if(!_controller.Moving) return;
            if(CurrentStamina > 0)
                Sprinting = true;
            Stopped = false;
            AddSprint();
        }
        if(Input.GetKeyUp(KeyCode.LeftShift)  && CurrentSprintTime <= 0)
        {
            PressingSprint = false;
            if (Sprinting && !Stopped)
            {
                Stopped = true;
                Sprinting = false;
                RemoveSprint();
                currentCanRegen = maxCanRegen;
            }
        }
        currentCanRegen -= Time.deltaTime;
        if (CurrentSprintTime > 0)
        {
            CurrentSprintTime -= Time.deltaTime;
        }
        if (CurrentStamina > MaxStamina)
        {
            CurrentStamina = MaxStamina;
        }

        if (PressingSprint)
        {
            if (_controller.Moving && !Sprinting)
            {
                if(CurrentStamina > 0)
                    Sprinting = true;
                Stopped = false;
                AddSprint();
            }
            else if (Sprinting && !Stopped && !_controller.Moving)
            {
                Stopped = true;
                Sprinting = false;
                RemoveSprint();
                currentCanRegen = maxCanRegen;
            }
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
            CurrentSprintTime = CantSprintTime;
            playerIcon.AddIcon(CantSprintTime , cantSprintIcon , "cant Sprint" , true);
            RemoveSprint();
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
        _controller.SpeedMultiplier += SprintMultiplier;
    }

    void RemoveSprint()
    {
        _controller.SpeedMultiplier -= SprintMultiplier;
    }
    
}
