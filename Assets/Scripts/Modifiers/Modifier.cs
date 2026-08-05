using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Modifier : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button Select;
    [SerializeField] private Image Icon;
    [SerializeField] private bool Selected;
    public bool Pressed;
    [SerializeField] private GameObject SelectedIndicator;
    [SerializeField] private GameObject selectedItem;
    public ModifierScriptableObject modifier;
    
    public void SetIcon()
    {
        Icon.sprite = modifier.modifierIcon;
    }

    public void Setmodifier(ModifierScriptableObject Setmodifier)
    {
        modifier = Setmodifier;
        SetIcon();
    }

    public void SelectThisModifier()
    {
        var manager = FindFirstObjectByType<ModifierUIManager>();
        manager.resetOthers();
        Debug.Log("Pressed");
        if (Pressed) return;
        SelectedIndicator.SetActive(true);
        Pressed = true;
        ModifierHolder holder = FindFirstObjectByType<ModifierHolder>();
        Debug.Log(holder.name);
        int index = modifier.indexValue;
        //holder.AddModfierWithIndexRpc(index);
        manager.ReadyUp(index);
    }

    public void UnPress()
    {
        Pressed = false;
        SelectedIndicator.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Selected = true;
        FindFirstObjectByType<ModifierDescriptionManager>().SetDescriptionByModifier(modifier);
        selectedItem.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Selected = false;
        FindFirstObjectByType<ModifierDescriptionManager>().SetDescriptionNull();
        selectedItem.SetActive(false);
    }
}
