using UnityEngine;

[System.Serializable]
public class SaveData
{
    public float cameraSensitivity;

    public SaveData(EscapeMenu escapeMenu)
    {
        cameraSensitivity = escapeMenu.SensitivitySlider.value;
    }
}
