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
    public Slider SensitivitySlider;
    public CameraController CameraController;
    [SerializeField] private TextMeshProUGUI SensitivityText;
    [SerializeField] private SoundManager soundManager;

    public Slider EnviromentSoundSlider;
    [SerializeField] private TextMeshProUGUI EnviromentSoundText;
    public Slider SoundSlider;
    [SerializeField] private TextMeshProUGUI SoundText;
    public bool Pausing = false;
    private bool Saved = false;

    
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
        SaveData data = PlayerSaveSystem.LoadPlayer();
        if (data == null)
        {
            SensitivitySlider.value = 350;
            PlayerSaveSystem.SavePlayer(this);
            return;
        }
        Debug.Log("Loaded Sens = " + data.cameraSensitivity);
        SensitivitySlider.value = data.cameraSensitivity;
        SoundSlider.value = data.soundVolume;
        EnviromentSoundSlider.value = data.Enviroment_Volume;
        Saved = false;
    }

    public void GetAllRefrences()
    {
        CameraController = player.GetComponent<CameraController>();
    }

    public void SetCameraController(CameraController cameraController)
    {
        CameraController = cameraController;
    }
    
    public void Update()
    {
        //Escape Detection
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!Pausing)
            {
                Saved = false;
                SetGui(true);
            }
            else
            {
                Saved = false;
                if (!Saved)
                {
                    PlayerSaveSystem.SavePlayer(this);
                    Saved = true;
                }
                SetGui(false);
            }
        }

        if (!GameManager.Instance.GameOver)
        {
            if (Pausing)
            {
                CameraController.CameraSensitivity = SensitivitySlider.value;
                SensitivityText.text = $"Sensitivity: {CameraController.CameraSensitivity}";
                
                soundManager.EnviromentSoundVolume = EnviromentSoundSlider.value;
                EnviromentSoundText.text = $"Enviorment Sound Volume: {EnviromentSoundSlider.value}";
                
                soundManager.soundVolume = SoundSlider.value;
                SoundText.text = $"Sound Volume: {SoundSlider.value}";
                CameraController.CanMoveCamera = false;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                CameraController.CameraSensitivity = SensitivitySlider.value;
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
            CameraController.CanMoveCamera = false;
        }
    }

    public void BackButton()
    {
        SetGui(false);
        if (!Saved)
        {
            PlayerSaveSystem.SavePlayer(this);
            Saved = true;
        }
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
            var modifierHolder = FindFirstObjectByType<ModifierHolder>();
            if(modifierHolder != null) modifierHolder.gameObject.GetComponent<NetworkObject>().Despawn(true);
            NetworkManager.Singleton.Shutdown();
            var Readyups = FindFirstObjectByType<ReadyUp>();
            if(Readyups != null) Destroy(Readyups.GameObject());
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
        var modifierHolder = FindFirstObjectByType<ModifierHolder>();
        if(modifierHolder != null) modifierHolder.gameObject.GetComponent<NetworkObject>().Despawn(true);
        NetworkManager.Singleton.Shutdown();
        var Readyups = FindFirstObjectByType<ReadyUp>();
        if(Readyups != null) Destroy(Readyups.GameObject());
        Destroy(NetworkManager.Singleton.GameObject());
        SceneManager.LoadScene(Loader.Scene.Lobby.ToString());
    }

    public void LeaveGame()
    {
        NetworkManager.Singleton.Shutdown();
        Destroy(NetworkManager.Singleton.GameObject());
        var Readyups = FindFirstObjectByType<ReadyUp>();
        if(Readyups != null) Destroy(Readyups.GameObject());
        var modifierHolder = FindFirstObjectByType<ModifierHolder>();
        if(modifierHolder != null) modifierHolder.gameObject.GetComponent<NetworkObject>().Despawn(true);
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
