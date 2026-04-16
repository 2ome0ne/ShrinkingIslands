using UnityEngine;
using TMPro;
public class ModifierDescriptionManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI modifierDescriptionText;
    public void SetDescriptionByModifier(ModifierScriptableObject modifier)
    {
        modifierDescriptionText.gameObject.SetActive(true);
        modifierDescriptionText.text = modifier.modifierDescription;
    }

    public void SetDescriptionNull()
    {
        modifierDescriptionText.gameObject.SetActive(false);
    }
}
