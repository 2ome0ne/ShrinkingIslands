using UnityEngine;
using UnityEngine.UI;
public class PlayerUImanager : MonoBehaviour
{
    [Header("--Player PickUp system UI--")]
    
    public Slider ThrowForceSlider;

    public void EnableDisableThrowForceSlider(bool value)
    {
        if (value)
        {
            ThrowForceSlider.gameObject.SetActive(true);
        }
        else
        {
            ThrowForceSlider.gameObject.SetActive(false);
        }
    }

    public void SetThrowForceSlider(float value)
    {
        ThrowForceSlider.value = value;
    }
}
