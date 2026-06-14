using UnityEngine;

[System.Serializable]
public class Recipie
{
    public string Item1;
    public string Item2;

    public GameObject ItemOutPut;
}

[System.Serializable]
public class ItemValue
{
    public string ItemName;
    public GameObject ItemPrefab;
    public float _ItemValue;
    public float _ItemNeededValue;
}

[CreateAssetMenu(fileName = "Recipie", menuName = "New Recipie", order = 1)]
public class CraftingRecpiesScriptableObject : ScriptableObject
{
    public Recipie[] recipies;
    
    public ItemValue[] itemValues;
}
