using System;
using UnityEngine;

public class ForgeInteractor : MonoBehaviour
{
    [SerializeField] private LayerMask forgeLayer;

    [SerializeField] private CameraController cameraController;
    public bool LookingAtForge = false;
    private void Update()
    {
        LookingAtForge = Physics.Raycast(cameraController.Camera.position, cameraController.Camera.forward, 2.5f,
            forgeLayer);
    }
}
