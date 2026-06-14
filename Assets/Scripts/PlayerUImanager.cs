using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
public class PlayerUImanager : NetworkBehaviour
{
    [Header("--Player PickUp system UI--")]
    
    public Slider ThrowForceSlider;
    public Slider StaminaSlider;
    public override void OnNetworkSpawn()
    {
        ThrowForceSlider.gameObject.SetActive(false);
    }

    public void EnableDisableThrowForceSlider(bool value)
    {
        if (value)
        {
            ThrowForceSlider.gameObject.SetActive(true);
        }
        else
        {
            ThrowForceSlider.gameObject.SetActive(false);
            Debug.Log("CLOSE");
        }
    }

    public void SetThrowForceSlider(float value)
    {
        ThrowForceSlider.value = value;
    }
}
