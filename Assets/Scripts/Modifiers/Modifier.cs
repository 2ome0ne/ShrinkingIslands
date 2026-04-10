using UnityEngine;
using UnityEngine.UI;

public class Modifier : MonoBehaviour
{
    [SerializeField] private Button Select;
    [SerializeField] private Image Icon;
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
        Debug.Log("Pressed");
        ModifierHolder holder = FindFirstObjectByType<ModifierHolder>();
        Debug.Log(holder.name);
        int index = modifier.indexValue;
        holder.AddModfierWithIndexRpc(index);
    }
}
