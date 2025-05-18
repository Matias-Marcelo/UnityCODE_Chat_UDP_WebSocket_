using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveManager
{
    private static string GetSavePath(string slotName)
    {
        return Application.persistentDataPath + "/" + slotName + ".save";
    }

    public static void SaveDataPlayer(DataToSave data, string slotName)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = GetSavePath(slotName);
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static DataToSave LoadDataPlayer(string slotName)
    {
        string path = GetSavePath(slotName);
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            DataToSave data = formatter.Deserialize(stream) as DataToSave;
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Archivo de guardado no encontrado en: " + path);
            return null;
        }
    }

    public static bool SaveExists(string slotName)
    {
        return File.Exists(GetSavePath(slotName));
    }
}

