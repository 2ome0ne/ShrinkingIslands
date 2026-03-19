using UnityEngine;
using UnityEngine.UI;
public class StaminaSystem : MonoBehaviour
{
    [Header("Stamina")] 
    public float CurrentStamina;
    public float MaxStamina;
    public float StaminaRegen;
    
    public float SprintMultiplier;
    
    public bool Sprinting = false;
    [Header("References")]
    [SerializeField] private Slider StaminaSlider;
    [SerializeField] private CharecterController _controller;
    [SerializeField] private PlayerAbillites _playerAbillites;

    void Start()
    {
        CurrentStamina = MaxStamina;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if(CurrentStamina > 0)
                Sprinting = true;
            AddSprint();
        }
        if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            Sprinting = false;
            AddSprint();
        }

        if (Sprinting == true)
        {
            CurrentStamina -= Time.deltaTime;
        }
        StaminaSlider.value = CurrentStamina;
        StaminaSlider.maxValue = MaxStamina;
        if (CurrentStamina < MaxStamina &&!_playerAbillites.Blocking)
        {
            CurrentStamina += StaminaRegen * Time.deltaTime;
        }
        else if(_playerAbillites.Blocking)
        {
            CurrentStamina -= StaminaRegen * Time.deltaTime;
        }
    }

    public void EatStamina(float amount)
    {
        CurrentStamina -= amount;
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
