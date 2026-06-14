using Unity.Netcode;
using UnityEngine;

public class PlayerPose : NetworkBehaviour
{
    [Header("Pose Refrences")] [SerializeField]
    private Transform CameraRot;

    [SerializeField] private GameObject GTX;
    [SerializeField] private GameObject GTX1;
    [SerializeField] private GameObject GTX2;
    [SerializeField] private GameObject ClothArmL;
    [SerializeField] private GameObject ClothArmLArmR;
    [SerializeField] private int PlayerColor;

    [SerializeField] private Material[] Shirt_Materials;

    public Transform Head;

    [SerializeField] private Quaternion HeadRot;
    // Start is called once before the first execution of Update after the MonoBehaviour is create
    

    [Rpc(SendTo.Everyone , InvokePermission = RpcInvokePermission.Everyone)]
    public void SetColorRpc()
    {
        PlayerColor = GetComponent<ThePlayerData>().IndexColor;
        Debug.Log("PLAYER CLOTH IS = " + PlayerColor);
        GTX2.GetComponent<SkinnedMeshRenderer>().material = Shirt_Materials[PlayerColor]; 
        ClothArmL.GetComponent<MeshRenderer>().material = Shirt_Materials[PlayerColor]; 
        ClothArmLArmR.GetComponent<MeshRenderer>().material = Shirt_Materials[PlayerColor]; 
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            GTX.SetActive(false);
            GTX1.SetActive(false);
            GTX2.SetActive(false);
        }
        else if(!IsOwner)
        {
            //Head.rotation = HeadRot;
        }
    }
}
