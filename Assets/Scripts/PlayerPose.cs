using Unity.Netcode;
using UnityEngine;

public class PlayerPose : NetworkBehaviour
{
    [Header("Pose Refrences")] [SerializeField]
    private Transform CameraRot;

    [SerializeField] private GameObject GTX;
    [SerializeField] private GameObject GTX1;
    [SerializeField] private GameObject GTX2;

    public Transform Head;

    [SerializeField] private Quaternion HeadRot;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
