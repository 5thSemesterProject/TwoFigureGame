using UnityEngine;
using UnityEditor;
using System.Collections;
using Unity.VisualScripting;

public class TwineStoryWizard : ScriptableWizard
{
    public TwineStoryData twineStoryData;
    string savePath = Application.dataPath + "/4_TwineStories/1_Content/1_JSON";


    void OnWizardUpdate()
    {
    }

    void OnWizardCreate()
    {
       
       //Save new Asset
       SaveSystem.SaveData(twineStoryData,savePath+ "/"+twineStoryData.title+".json");

        /*string[] twnNames = new string[2] { "testName", "testName" };
        TwineStoryNames twN = new TwineStoryNames(twnNames);
        SaveSystem.SaveData<TwineStoryNames>(twN,savePath+ "/TwineStoryNames.json");*/
    }

    [MenuItem("Window/Create Twine Story Asset")]
    static void RenderCubemap()
    {
        ScriptableWizard.DisplayWizard<TwineStoryWizard>(
            "Create Twine Story Asset", "Save");
    }
}