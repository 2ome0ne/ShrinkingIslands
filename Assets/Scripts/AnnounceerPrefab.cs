using TMPro;
using UnityEngine;

public class AnnounceerPrefab : MonoBehaviour
{
    public TextMeshProUGUI announceText;

    public void Announce(string text)
    {
        announceText.text = "-" + text + "-";
    }
}
