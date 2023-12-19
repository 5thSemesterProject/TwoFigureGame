using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.IO;
using System.Linq;

[RequireComponent(typeof(Slider))]
[ExecuteInEditMode]
public class UpdateArchiveSlider : MonoBehaviour
{
    private Slider archiveProgress;
    [SerializeField] private int maxObjects;
    [SerializeField] private int ownedObjects;

    private void OnEnable()
    {
        LoadTwineDataIntoSlider();
    }

    private void LoadTwineDataIntoSlider()
    {
        string[] addressablePaths = GetAllTwineNamesWithAddressablePath();
        List<string> twineList = addressablePaths.ToList<string>();
        AsyncOperationHandle<IList<TextAsset>> asyncOperationHandle = Addressables.LoadAssetsAsync<TextAsset>(twineList, null, Addressables.MergeMode.Union);

        if (archiveProgress == null)
            archiveProgress = GetComponent<Slider>();

        asyncOperationHandle.Completed += UpdateProgress;
    }

    private static string[] GetAllTwineNamesWithAddressablePath()
    {
        string addressablePath = "Assets/9_TwineStories/1_Content/1_JSON/";
        string folderPath = Application.dataPath + "/9_TwineStories/1_Content/1_JSON";
        string[] filePaths = Directory.GetFiles(folderPath);
        string[] fileNames = new string[filePaths.Length];

        for (int i = 0; i < fileNames.Length; i++)
        {
            string fileName = Path.GetFileName(filePaths[i]);

            // Exclude files with ".meta" extension and load only ".json" files
            if (!fileName.EndsWith(".meta") && fileName.EndsWith(".json"))
            {
                filePaths[i] = addressablePath + fileName;
            }
            else
            {
                filePaths[i] = null; // Mark meta files or non-JSON files as null
            }
        }

        // Filter out null entries (meta files or non-JSON files)
        filePaths = filePaths.Where(path => path != null).ToArray();

        return filePaths;
    }

    private void UpdateProgress(AsyncOperationHandle<IList<TextAsset>> storyData)
    {
        List<TwineStoryData> twineStoryDatas = new List<TwineStoryData>();

        foreach (var textAsset in storyData.Result)
            twineStoryDatas.Add(JsonUtility.FromJson<TwineStoryData>(textAsset.ToString()));

        maxObjects = twineStoryDatas.Count;

        int count = 0;
        for (int i = 0; i < twineStoryDatas.Count; i++)
        {
            if (twineStoryDatas[i].unlocked == true)
            {
                count++;
            }
        }

        ownedObjects = Mathf.Clamp(count, 0, maxObjects);

        UpdateSlider();
    }

    public void UpdateSlider()
    {
        float value = (float)ownedObjects / (float)maxObjects;
        value = Mathf.Clamp01(value);
        archiveProgress.value = value;
    }
}
