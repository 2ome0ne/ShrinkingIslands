using Unity.Netcode;
using UnityEngine;

public class SmallIslandTile : NetworkBehaviour
{
    [SerializeField] private GameObject[] ListOfGTXs;

    [Rpc(SendTo.Everyone)]
    public void Set_a_GTXRpc()
    {
        int random = Random.Range(0, 2);
        ListOfGTXs[random].SetActive(true);    
    }
}
