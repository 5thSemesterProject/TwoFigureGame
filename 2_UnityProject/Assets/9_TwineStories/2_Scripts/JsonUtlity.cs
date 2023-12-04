using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class JsonUtlity : MonoBehaviour
{
    /// <summary>
    /// Saves the data to a file at the specified path.
    /// </summary>
    /// <param name="saveFile">The SaveData object to save.</param>
    /// <param name="savePath">The path to save the file.</param>
    public void SaveData(SaveData saveFile, string savePath)
    {
        string json = JsonUtility.ToJson(saveFile);
       
        try
        {
            using StreamWriter writer = new StreamWriter(savePath);
            writer.Write(json);
            Debug.Log("SavedData to " + savePath);
        }
        catch (IOException)
        {
            Debug.Log("File is in use. Trying again");
            SaveData(saveFile, savePath);
        }
    }

    /// <summary>
    /// Loads the data from a file at the specified path.
    /// </summary>
    /// <typeparam name="T">The type of the SaveData to load.</typeparam>
    /// <param name="savePath">The path to load the file from.</param>
    /// <returns>The loaded SaveData object or null if loading fails.</returns>
    public T LoadData<T>(string savePath) where T:SaveData
    {
        using StreamReader reader = new StreamReader(savePath);
        string json = reader.ReadToEnd();

        try
        {
            return JsonUtility.FromJson<T>(json);
        }
        catch(System.Exception)
        {
            return null;
        }
        
    }
}
