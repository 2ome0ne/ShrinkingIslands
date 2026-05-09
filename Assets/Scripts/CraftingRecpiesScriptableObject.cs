using UnityEngine;

[System.Serializable]
public class Recipie
{
    public string Item1;
    public string Item2;

    public GameObject ItemOutPut;
}

[CreateAssetMenu(fileName = "Recipie", menuName = "New Recipie", order = 1)]
public class CraftingRecpiesScriptableObject : ScriptableObject
{
    public Recipie[] recipies;
}
