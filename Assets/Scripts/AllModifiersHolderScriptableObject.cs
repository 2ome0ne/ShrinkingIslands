using UnityEngine;

[CreateAssetMenu(fileName = "AllModifiersHolderScriptableObject", menuName = "AllModifiersHolderScriptableObject")]
public class AllModifiersHolderScriptableObject : ScriptableObject
{
    public ModifierScriptableObject[] AllModifiers;
}
