using System;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class ForgeInteractor : NetworkBehaviour
{
    [SerializeField] private LayerMask forgeLayer;
    
    [SerializeField] private TextMeshProUGUI LeftHandValueText;
    [SerializeField] private TextMeshProUGUI RightHandValueText;
    [SerializeField] private CraftingRecpiesScriptableObject craftingRecipes;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private PickUpSystem pickUpSystem;
    [SerializeField] private GearManager gearManager;
    [SerializeField] private float DetectionRange = 3.4f;
    public bool LookingAtForge = false;
    public GameObject lookingForge;
    private void Update()
    {
        LookingAtForge = Physics.Raycast(cameraController.Camera.position, cameraController.Camera.forward , out var hit, DetectionRange,
            forgeLayer);
        if (LookingAtForge)
        {
            if (pickUpSystem.HasItem.Value && IsOwner)
            {
                foreach (var item in craftingRecipes.itemValues)
                {
                    RightHandValueText.text = item._ItemValue.ToString();
                }
            }
            else
            {
                RightHandValueText.text = "";
            }
            
            if (gearManager.HasGear && IsOwner)
            {
                foreach (var item in craftingRecipes.itemValues)
                {
                    LeftHandValueText.text = item._ItemValue.ToString();
                }
            }
            else
            {
                LeftHandValueText.text = "";
            }
            
            lookingForge = hit.collider.gameObject;
        }
        else
        {
            RightHandValueText.text = "";
            LeftHandValueText.text = "";
        }
        
    }
}
