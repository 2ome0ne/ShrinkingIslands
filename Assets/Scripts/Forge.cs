using System;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

public class Forge : NetworkBehaviour
{
    [SerializeField] private CraftingRecpiesScriptableObject craftingRecipes;

    [SerializeField] private float currentForgeValue;
    
    [SerializeField] private string currentItem1;
    [SerializeField] private string currentItem2;
    
    [SerializeField] private Transform item1;
    [SerializeField] private Transform item2;

    [SerializeField] private Transform content1;
    [SerializeField] private Transform content2;
    [SerializeField] private GameObject CombineParticle;

    [SerializeField] private Transform AllContent;

    [SerializeField] private float rotationSpeed = 10;
    [SerializeField] private float sinSpeed = 5;
    [SerializeField] private float sinLenght = 1.4f;
    [SerializeField] private float contentAwayFromMiddle = 3;
    [SerializeField] private float maxContentAwayFrom = 3;
    [SerializeField] private float maxCombineRotationSpeed = 20;
    [SerializeField] private float minCombineRotationSpeed = 10;

    [SerializeField] private GameObject resultItem;

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
                if (IsServer)
                {
                    CompleteCraftingRecipeRpc();
                    Debug.Log("Can Access");
                }
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


    [Rpc(SendTo.Server , InvokePermission = RpcInvokePermission.Everyone)]
    public void PutInForgeRpc(NetworkObjectReference netObjRef)
    {
        GameManager.Instance.soundManager.SpawnSoundRpc(transform.position, 5 , 1 , 1 , 13);
        Debug.Log("PuT IN FORGE");
        netObjRef.TryGet(out NetworkObject item);
        SetFollowForAllRpc(item);

        CheckForCraftsRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void SetFollowForAllRpc(NetworkObjectReference netObjRef)
    {
        netObjRef.TryGet(out NetworkObject item);
        Debug.Log("SetFollowForAllRpc" + item.name);
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
    }
    
    [Rpc(SendTo.Server )]
    public void CheckForCraftsRpc()
    {
        currentForgeValue = 0;
        foreach (var item in craftingRecipes.itemValues)
        {
            if (item.ItemName == currentItem1)
            {
                currentForgeValue += item._ItemValue;
            }
            if (item.ItemName == currentItem2)
            {
                currentForgeValue += item._ItemValue;
                resultItem = GetRandomItemFromValues();
                Debug.Log("Have A RESULT " + resultItem.name);
                CombineOnEVERYONERpc();
                GameManager.Instance.soundManager.SpawnSoundRpc(transform.position, 10 , 1 , 1 , 14);
                //CompleteCraftingRecipeRpc();
                return;
            }
        }
        //currentForgeValue += currentItem1
    }

    [Rpc(SendTo.Everyone)]
    private void CombineOnEVERYONERpc()
    {
        Combining = true;
    }

    [Rpc(SendTo.Server , InvokePermission = RpcInvokePermission.Everyone)]
    private void CompleteCraftingRecipeRpc()
    {
        SpawnRpc();
        Debug.Log("COMPLETE FORGING");
        //netObjRef.TryGet(out NetworkObject resultItem);
        GameObject result = Instantiate(resultItem, AllContent.position, Quaternion.identity);
        result.GetComponent<NetworkObject>().Spawn();
        ResetForge();
    }

    [Rpc(SendTo.Everyone)]
    private void SpawnRpc()
    {
        Instantiate(CombineParticle , AllContent.position , Quaternion.identity);
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

    private GameObject GetRandomItemFromValues()
    {
        float currentMaxValue = currentForgeValue;

        int result = 0;

        for (int i = 0; i < craftingRecipes.itemValues.Length; i++)
        {
            Debug.Log("BAAAAAAAAAAAAA"+i);
            if (craftingRecipes.itemValues[i]._ItemNeededValue <= currentMaxValue)
            {
                float random = UnityEngine.Random.Range(0, currentMaxValue);
                if (craftingRecipes.itemValues[i]._ItemNeededValue > random)
                {
                    Debug.Log("Random IS = " + random);
                    result = i;
                }
            }
        }

        return craftingRecipes.itemValues[result].ItemPrefab;
    }
}
