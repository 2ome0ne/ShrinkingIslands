using Unity.Netcode;
using UnityEngine;

public class Forge : NetworkBehaviour
{
    [SerializeField] private CraftingRecpiesScriptableObject craftingRecipes;

    [SerializeField] private string currentItem1;
    [SerializeField] private string currentItem2;

    [SerializeField] private Transform content1;
    [SerializeField] private Transform content2;


    [Rpc(SendTo.Server)]
    public void PutInForgeRpc(NetworkObjectReference netObjRef)
    {
        netObjRef.TryGet(out NetworkObject item);
        if (currentItem1 != null)
        {
            FollowTransform FT = item.GetComponent<FollowTransform>();
            currentItem2 = FT.ItemName;
            FT.SetTargetTransform(content2 , null);
        }
        else
        {
            FollowTransform FT = item.GetComponent<FollowTransform>();
            currentItem1 = FT.ItemName;
            FT.SetTargetTransform(content1 , null);
        }

        CheckForCrafts();
    }
    
    public void CheckForCrafts()
    {
        foreach (var recipie in craftingRecipes.recipies)
        {
            if (CheckEqual(recipie, currentItem1))
            {
                CompleteCraftingRecipeRpc(recipie.ItemOutPut);
                return;
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void CompleteCraftingRecipeRpc(NetworkObjectReference netObjRef)
    {
        netObjRef.TryGet(out NetworkObject resultItem);
        
    }

    private void ResetForge()
    {
        currentItem1 = null;
        currentItem2 = null;
    }

    private bool CheckEqual(Recipie input, string expected)
    {
        if (input.Item1.Equals(expected)) return true;
        if (input.Item2.Equals(expected)) return true;
        else return false;
    }
}
