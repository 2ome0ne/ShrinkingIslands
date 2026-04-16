using System;
using UnityEngine;
using UnityEngine.UI;
public class IconCooldownHolder : MonoBehaviour
{
    [SerializeField] private Image Itemicon;
    [SerializeField] private Image ItemBackground;
    [SerializeField] private Slider CooldownSlider;
    
    public Sprite icon;
    public string id;
    public float currentCooldown;
    public bool cooldownActive;

    public PlayerIconShower Shower;

    public void SetMaxCooldown(float cooldown)
    {
        CooldownSlider.maxValue = cooldown;
    }

    private void Update()
    {
        if (cooldownActive)
        {
            CooldownSlider.value = currentCooldown;
            currentCooldown -= Time.deltaTime;
        }

        if (currentCooldown <= 0)
        {
            Shower.DestoryIcon(this.gameObject);
        }
    }


    public void SetIconSprite()
    {
        Itemicon.sprite = icon;
        ItemBackground.sprite = icon;
    }
}
