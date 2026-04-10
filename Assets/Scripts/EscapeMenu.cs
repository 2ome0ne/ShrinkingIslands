using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class EscapeMenu : NetworkBehaviour
{
    [SerializeField] private GameObject escapeMenu;
    public GameObject player;
    [SerializeField] private Slider SensitivitySlider;
    [SerializeField] private CameraController CameraController;
    [SerializeField] private TextMeshProUGUI SensitivityText;
    public bool Pausing = false;

    
    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnect;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnDisconnect;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnDisconnect;
        }
    }

    public override void OnNetworkSpawn()
    {
        GameManager.Instance.AddEscapeToAllRpc();
    }

    public void GetAllRefrences()
    {
        CameraController = player.GetComponent<CameraController>();
    }

    
    
    public void Update()
    {
        //Escape Detection
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!Pausing)
            {
                SetGui(true);
            }
            else
            {
                SetGui(false);
            }
        }

        if (!GameManager.Instance.GameOver)
        {
            if (Pausing)
            {
                CameraController.CameraSensitivity = SensitivitySlider.value;
                SensitivityText.text = $"Sensitivity: {CameraController.CameraSensitivity}";
                CameraController.CanMoveCamera = false;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                if(CameraController != null)
                    CameraController.CanMoveCamera = true;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void BackButton()
    {
        SetGui(false);
    }

    public void Disconnect()
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("ClearALlClients");
            DisconenctAllConnectedClientRpc();
            Invoke(nameof(LeaveGame), 0.1f);
        }
        else
        {
            Debug.Log("Didnt Return >:(");
            NetworkManager.Singleton.Shutdown();
            var Readyups = FindFirstObjectByType<ReadyUp>();
            if(Readyups != null) Destroy(Readyups.GameObject());
            var modifierHolder = FindFirstObjectByType<ModifierHolder>();
            if(modifierHolder != null) Destroy(modifierHolder.GameObject());
            Destroy(NetworkManager.Singleton.GameObject());
            SceneManager.LoadScene(Loader.Scene.Lobby.ToString());
        }
        
        RelayManager relay = FindFirstObjectByType<RelayManager>();
        Destroy(relay.gameObject);
        //Destroy(NetworkManager.Singleton.gameObject);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    [ClientRpc]
    private void DisconenctAllConnectedClientRpc()
    {
        if (IsHost) return;
        Debug.Log("Didnt Return >:(");
        NetworkManager.Singleton.Shutdown();
        var Readyups = FindFirstObjectByType<ReadyUp>();
        if(Readyups != null) Destroy(Readyups.GameObject());
        var modifierHolder = FindFirstObjectByType<ModifierHolder>();
        if(modifierHolder != null) Destroy(modifierHolder.GameObject());
        Destroy(NetworkManager.Singleton.GameObject());
        SceneManager.LoadScene(Loader.Scene.Lobby.ToString());
    }

    public void LeaveGame()
    {
        NetworkManager.Singleton.Shutdown();
        Destroy(NetworkManager.Singleton.GameObject());
        var Readyups = FindFirstObjectByType<ReadyUp>();
        if(Readyups != null) Destroy(Readyups.GameObject());
        SceneManager.LoadScene(Loader.Scene.Lobby.ToString());
    }

    private void OnDisconnect(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId || clientId == 0)
        {
            //SceneManager.LoadScene(Loader.Scene.Lobby.ToString());
        }
    }


    private void SetGui(bool value)
    {
        Pausing = value;
        escapeMenu.SetActive(value);
    }
}
