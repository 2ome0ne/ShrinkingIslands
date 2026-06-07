using System;
using Unity.Netcode;
using UnityEngine;

public class NoReapeatManager : MonoBehaviour
{
    public static NoReapeatManager _instance;
    private void Awake()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Cleanup();
    }
    
    void Cleanup()
    {
        if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
