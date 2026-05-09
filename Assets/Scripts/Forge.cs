using System;
using Unity.Netcode;
using UnityEngine;

public class Forge : NetworkBehaviour
{
    [SerializeField] private CraftingRecpiesScriptableObject craftingRecipes;

    [SerializeField] private string currentItem1;
    [SerializeField] private string currentItem2;
    
    [SerializeField] private Transform item1;
    [SerializeField] private Transform item2;

    [SerializeField] private Transform content1;
    [SerializeField] private Transform content2;

    [SerializeField] private Transform AllContent;

    [SerializeField] private float rotationSpeed = 10;
    [SerializeField] private float sinSpeed = 5;
    [SerializeField] private float sinLenght = 1.4f;
    [SerializeField] private float contentAwayFromMiddle = 3;
    [SerializeField] private float maxContentAwayFrom = 3;
    [SerializeField] private float maxCombineRotationSpeed = 20;
    [SerializeField] private float minCombineRotationSpeed = 10;

    private GameObject resultItem;

    [SerializeField] private bool Combining = false;

    private void Update()
    {
        //Moving for contents
        AllContent.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        //content1.position = new Vector3(content1.position.x,sinLenght * Mathf.Sin(Time.time * sinSpeed) + AllContent.position.y, content2.position.z);
        //content2.position = new Vector3(content2.position.x,sinLenght * Mathf.Cos(Time.time * sinSpeed) + AllContent.position.y, content2.position.z);
        Vector3 calContentVector = AllContent.position;
        calContentVector += content2.forward * contentAwayFromMiddle;
        calContentVector += new Vector3(0, sinLenght * Mathf.Cos(Time.time * sinSpeed), 0) ;
        content2.position = calContentVector;
        
        Vector3 calContent1Vector = AllContent.position;
        calContent1Vector -= content2.forward * contentAwayFromMiddle;
        calContent1Vector += new Vector3(0, sinLenght * Mathf.Sin(Time.time * sinSpeed), 0);
        content1.position = calContent1Vector;
        
        if (Combining)
        {
            contentAwayFromMiddle = Mathf.Lerp(contentAwayFromMiddle, 0, Time.deltaTime * 1.5f);
            rotationSpeed = Mathf.Lerp(rotationSpeed, maxCombineRotationSpeed, Time.deltaTime * 5f);
            if (contentAwayFromMiddle < 0.01f)
            {
                CompleteCraftingRecipeRpc();
                Combining = false;
            }
        }
        else
        {
            if(contentAwayFromMiddle > maxContentAwayFrom - 0.01f) return;
            contentAwayFromMiddle = Mathf.Lerp(contentAwayFromMiddle, maxContentAwayFrom, Time.deltaTime);
            rotationSpeed = Mathf.Lerp(rotationSpeed, minCombineRotationSpeed, Time.deltaTime);
        }
        
    }


    [Rpc(SendTo.Server)]
    public void PutInForgeRpc(NetworkObjectReference netObjRef)
    {
        netObjRef.TryGet(out NetworkObject item);
        if (item1 != null)
        {
            FollowTransform FT = item.GetComponent<FollowTransform>();
            currentItem2 = FT.ItemName;
            item2 = item.transform;
            FT.SetTargetTransform(content2 , null);
        }
        else
        {
            FollowTransform FT = item.GetComponent<FollowTransform>();
            currentItem1 = FT.ItemName;
            item1 = item.transform;
            FT.SetTargetTransform(content1 , null);
        }

        CheckForCrafts();
    }
    
    public void CheckForCrafts()
    {
        foreach (var recipie in craftingRecipes.recipies)
        {
            memeoryitem1 = false;
            memeoryitem2 = false;
            if (CheckEqual(recipie, currentItem1))
            {
                Debug.Log("Have A CHANCE");
                if (CheckEqual(recipie, currentItem2))
                {
                    Debug.Log("Have A RESULT " + recipie.ItemOutPut.name);
                    resultItem = recipie.ItemOutPut;
                    Combining = true;
                    //CompleteCraftingRecipeRpc();
                    return;
                }
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void CompleteCraftingRecipeRpc()
    {
        //netObjRef.TryGet(out NetworkObject resultItem);
        GameObject result = Instantiate(resultItem, AllContent.position, Quaternion.identity);
        result.GetComponent<NetworkObject>().Spawn();

        ResetForge();
    }

    private void ResetForge()
    {
        item2.GetComponent<NetworkObject>().Despawn();
        item1.GetComponent<NetworkObject>().Despawn();
        memeoryitem1 = false;
        memeoryitem2 = false;
        currentItem1 = null;
        currentItem2 = null;
        item2 = null;
        item1 = null;
    }

    private bool memeoryitem1 = false;
    private bool memeoryitem2 = false;
    private bool CheckEqual(Recipie input, string expected)
    {
        if (input.Item1 == expected && !memeoryitem1)
        {
            memeoryitem1 = true;
            return true;
        }
        if (input.Item2 == expected && !memeoryitem2)
        {
            memeoryitem2 = true;
            return true;
        }
        return false;
    }
}
