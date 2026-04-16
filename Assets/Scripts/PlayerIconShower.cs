using System.Collections.Generic;
using UnityEngine;

public class PlayerIconShower : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject iconPrefab;


    [SerializeField] private List<IconCooldownHolder> currentIcons;


    public void AddIcon(float DestoryTime, Sprite IconTexture , string Id , bool cooldown)
    {
        GameObject newIcon = Instantiate(iconPrefab, content);
        IconCooldownHolder icon = newIcon.GetComponent<IconCooldownHolder>();
        icon.cooldownActive = cooldown;
        icon.icon = IconTexture;
        icon.SetIconSprite();
        icon.id = Id;
        icon.SetMaxCooldown(DestoryTime);
        icon.currentCooldown = DestoryTime;
        icon.Shower = this;
    }

    public IconCooldownHolder FindIconWithId(string Id)
    {
        return currentIcons.Find(Icon => Icon.id == Id);
    }

    public void EditIcon(IconCooldownHolder newIcon, float DestoryTime)
    {
        newIcon.currentCooldown = DestoryTime;
        newIcon.SetMaxCooldown(DestoryTime);
    }

    public void DestoryIcon(GameObject SelectedIcon)
    {
        currentIcons.Remove(currentIcons.Find(Icon => Icon.gameObject == SelectedIcon));
        Destroy(SelectedIcon);
    }
}
