using UnityEngine;

[System.Serializable]
public class SaveData
{
    public float cameraSensitivity;
    public float soundVolume = 1;
    public float Enviroment_Volume = 1;

    public SaveData(EscapeMenu escapeMenu)
    {
        cameraSensitivity = escapeMenu.SensitivitySlider.value;
        soundVolume = escapeMenu.SoundSlider.value;
        Enviroment_Volume = escapeMenu.EnviromentSoundSlider.value;
    }
}
