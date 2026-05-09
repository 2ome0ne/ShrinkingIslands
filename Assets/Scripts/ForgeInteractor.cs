using System;
using UnityEngine;

public class ForgeInteractor : MonoBehaviour
{
    [SerializeField] private LayerMask forgeLayer;

    [SerializeField] private CameraController cameraController;
    [SerializeField] private float DetectionRange = 3.4f;
    public bool LookingAtForge = false;
    public GameObject lookingForge;
    private void Update()
    {
        LookingAtForge = Physics.Raycast(cameraController.Camera.position, cameraController.Camera.forward , out var hit, DetectionRange,
            forgeLayer);
        if(LookingAtForge) lookingForge = hit.collider.gameObject;
    }
}
