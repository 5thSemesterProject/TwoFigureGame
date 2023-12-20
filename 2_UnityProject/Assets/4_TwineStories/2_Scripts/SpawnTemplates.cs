using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTemplates : MonoBehaviour
{
    [SerializeField] Transform parent;

    [SerializeField] TwineStoryData[] twineStoryDatas;

    

    void Start()
    {
        LoadAssets();
    }

   void LoadAssets()
   {
        string[] fileNames = SaveSystem.GetFileNamesInDirectory(Application.dataPath+"/9_TwineStories/1_Content/1_JSON");
        twineStoryDatas = new TwineStoryData[fileNames.Length];
        
        for (int i = 0; i < twineStoryDatas.Length; i++)
        {
            twineStoryDatas[i] = SaveSystem.LoadData<TwineStoryData>(Application.dataPath+$"/9_TwineStories/1_Content/1_JSON/{fileNames[0]}");
        }
   }
}
