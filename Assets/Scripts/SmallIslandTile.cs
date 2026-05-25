using Unity.Netcode;
using UnityEngine;

public class SmallIslandTile : NetworkBehaviour
{
    [SerializeField] private GameObject[] ListOfGTXs;
    [SerializeField] private Transform[] ListOfForgeLocations;

    [SerializeField] private GameObject forge;

    [Rpc(SendTo.Everyone , InvokePermission = RpcInvokePermission.Everyone)]
    public void Set_a_GTXRpc(int random)
    {
        ListOfGTXs[random].SetActive(true);    
        if(!IsServer) return;
        GameObject Fg = Instantiate(forge , ListOfForgeLocations[random].position , Quaternion.identity);
        Fg.GetComponent<NetworkObject>().Spawn(true);
    }
}
