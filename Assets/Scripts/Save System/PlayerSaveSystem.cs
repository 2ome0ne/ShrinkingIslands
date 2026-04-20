using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
public class PlayerSaveSystem
{
    public static void SavePlayer(EscapeMenu escapeMenu)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/save.fun";
        using (FileStream stream = new FileStream(path, FileMode.Create))
        {
            SaveData data = new SaveData(escapeMenu);
            formatter.Serialize(stream, data);
            formatter.Serialize(stream, data);
            stream.Close();
        }
    }

    public static SaveData LoadPlayer()
    {
        string path = Application.persistentDataPath + "/save.fun";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            SaveData data = formatter.Deserialize(stream) as SaveData;
            stream.Close();
            return data;
            
        }
        else
        {
            Debug.Log("No save file found" + path);
            return null;
        }
    }
}
