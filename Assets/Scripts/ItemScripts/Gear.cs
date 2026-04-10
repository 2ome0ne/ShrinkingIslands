using UnityEngine;

[CreateAssetMenu(fileName = "Gear", menuName = "Items/Gear", order = 1)]
public class Gear : ScriptableObject
{
    public string Gear_name;
    public GameObject prefab;
}
