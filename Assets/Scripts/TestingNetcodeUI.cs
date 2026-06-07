using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
public class TestingNetcodeUI : MonoBehaviour
{
    [SerializeField] private Button startHost;
    [SerializeField] private Button StartClientButton;

    private void Awake()
    {
        startHost.onClick.AddListener(() =>
        {
            Debug.Log("Start Host");
            NetworkManager.Singleton.StartHost();
            Hide();
        });
        
        StartClientButton.onClick.AddListener(() =>
        {
            Debug.Log("Start Client");
            NetworkManager.Singleton.StartClient();
            Hide();
        });
        
    }
    
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}