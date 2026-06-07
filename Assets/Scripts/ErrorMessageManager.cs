using UnityEngine;
using UnityEngine.UIElements;
using TMPro;
public class ErrorMessageManager : MonoBehaviour
{
    public static ErrorMessageManager instance;
    [SerializeField] private GameObject errorPanel;
    [SerializeField] private TextMeshProUGUI errorText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowError(string message)
    {
        errorPanel.SetActive(true);
        errorText.text = message;
    }

    public void CloseErrorPanel()
    {
        errorPanel.SetActive(false);
    }
}
