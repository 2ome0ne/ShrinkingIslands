using UnityEngine;

[System.Serializable]
public class Recipie
{
    public string Item1;
    public string Item2;

    public GameObject ItemOutPut;
}

public class CraftingRecpiesScriptableObject : ScriptableObject
{
    public Recipie[] recipies;
}
