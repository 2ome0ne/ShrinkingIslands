using UnityEngine;

[CreateAssetMenu(fileName = "Modifier", menuName = "ModifierCreateModifier")]
public class ModifierScriptableObject : ScriptableObject
{
    public string modifierName;
    public string modifierDescription;
    public Sprite modifierIcon;
    public int indexValue;
}
