using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Unity.Netcode;

public class TutorialManager : MonoBehaviour
{
    public CharecterController controller;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private SoundManager soundManager;
    public bool gotPlayer;
    private bool teleported = false;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private GameObject tutorialPanel;

    [SerializeField] private GameObject itemTutorialstuff;
    [SerializeField] private GameObject RockBot;

    [SerializeField] private NetworkObject flintlock;
    [SerializeField] private NetworkObject rock;
    
    public Vector3 LastPlayerPosition;
    
    private void Start()
    {
        Invoke("waitASec" , 0.1f);
    }

    private void waitASec()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void SetTutorialText(string text)
    {
        tutorialPanel.SetActive(true);
        tutorialText.text = text;
    }

    private void Update()
    {
        if (controller == null && FindFirstObjectByType<CharecterController>())
        {
            controller = FindFirstObjectByType<CharecterController>();
            characterController = controller.GetComponent<CharacterController>();
            gotPlayer = true;
        }

        if (tutorialPanel.activeInHierarchy && Input.GetKeyDown(KeyCode.Z))
        {
            tutorialPanel.SetActive(false);
        }

        if (gotPlayer)
        {
            if (controller.IsGrounded)
            {
                LastPlayerPosition = controller.transform.position;
                teleported = false;
            }
            
            if (controller.transform.position.y < 0 && !teleported)
            {
                teleported = true;
                StartCoroutine(teleportPlayer());
            }
        }
        
    }

    public void RockBotDies()
    {
        itemTutorialstuff.SetActive(true);
        flintlock.Spawn(true);
        rock.Spawn(true);
        flintlock.TrySetParent(itemTutorialstuff);
        rock.TrySetParent(itemTutorialstuff);
    }

    IEnumerator teleportPlayer()
    {
        
        characterController.enabled = false;
        controller.transform.position = LastPlayerPosition;
        soundManager.SpawnSoundRpc(controller.transform.position , 10 , 0.7f , 1 , 9);
        yield return new WaitUntil(() => controller.transform.position == LastPlayerPosition);
        characterController.enabled = true;
    }
}
