using Unity.Netcode;
using UnityEngine;

public class CameraController : NetworkBehaviour
{

    [Header("Camera Settings")]
    [SerializeField] private float CameraSensitivity = 350f;
    [Header("Refrences")] 
    [SerializeField] private Transform Camera;
    [SerializeField] private Transform CamHolder;
    [SerializeField] private Transform Ppos;
    [SerializeField] private bool SpectatorCamera;
    private float PersonalxRotation;
    private float mouseY;
    void Start()
    {
        //Locking Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (!IsOwner)
        {
            Camera.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!IsOwner) return;
        float mouseX = Input.GetAxis("Mouse X") * CameraSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * CameraSensitivity * Time.deltaTime;

        PersonalxRotation -= mouseY;
        PoseRotateRpc(PersonalxRotation);
        HeadRotateRpc(PersonalxRotation);
        PersonalxRotation = Mathf.Clamp(PersonalxRotation, -90f, 90f);
        CamHolder.localRotation = Quaternion.Euler(PersonalxRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    [Rpc(SendTo.Everyone)]
    public void PoseRotateRpc(float xRotation)
    {
        xRotation = -xRotation;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        if(!SpectatorCamera)
            Ppos.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        CamHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    [Rpc(SendTo.NotMe)]
    public void HeadRotateRpc(float xRotation)
    {
        CamHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
