using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;

public class SaveSystem : MonoBehaviour
{

    /// <summary>
    /// Saves the data to a file at the specified path.
    /// </summary>
    /// <param name="saveFile">The SaveData object to save.</param>
    /// <param name="savePath">The path to save the file.</param>
    public static void SaveData<T>(T saveFile, string savePath)
    {
        string json = JsonUtility.ToJson(saveFile);
       
        try
        {
            using StreamWriter writer = new StreamWriter(savePath);
            writer.Write(json);
            Debug.Log("SavedData to " + savePath);
        }
        catch (IOException exception)
        {
            Debug.Log (exception.Message);
        }
    }


    public static async Task SaveDataAsync<T>(T saveFile, string savePath)
    {
        string json = JsonUtility.ToJson(saveFile);
       
        try
        {
            //using StreamWriter writer = new StreamWriter(savePath);
           // await writer.WriteAsync(json);
            Debug.Log("Test " + savePath);
        }
        catch (IOException exception)
        {
            Debug.Log (exception.Message);
        }
    }

    /// <summary>
    /// Loads the data from a file at the specified path.
    /// </summary>
    /// <typeparam name="T">The type of the SaveData to load.</typeparam>
    /// <param name="savePath">The path to load the file from.</param>
    /// <returns>The loaded SaveData object or null if loading fails.</returns>
    public static T LoadData<T>(string savePath) /*where T:SaveData*/
    {
        using StreamReader reader = new StreamReader(savePath);
        string json = reader.ReadToEnd();

        try
        {
            return JsonUtility.FromJson<T>(json);
        }
        catch(System.Exception)
        {
            return default;
        }
        
    }

    public static string[] GetFileNamesInDirectory(string directoryPath)
    {
        // Check if the directory exists
        if (Directory.Exists(directoryPath))
        {
            // Get all file names in the directory
            string[] fileNames = Directory.GetFiles(directoryPath);

            return fileNames;
        }
        else
        {
            // If the directory does not exist, return an empty array or handle the situation accordingly
            return new string[0];
        }
    }

}
