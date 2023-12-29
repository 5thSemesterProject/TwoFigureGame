using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class SaveTwine : MonoBehaviour
{
    Interactable interactable;
    [SerializeField]_1_JSON twineStory;

    TwineStoryData twineStoryData;
    string fullPath;
    string fileName;

    void  Start()
    {
        interactable = GetComponent<Interactable>();
        interactable.triggerEvent+=SaveToTwine;
        
        //Load Twine Story
        fileName = twineStory.ToString();
        fileName = RemoveFirstUnderscore(fileName);
        fullPath = Application.dataPath+$"/4_TwineStories/1_Content/1_JSON/{fileName}.json";
        twineStoryData = SaveSystem.LoadData<TwineStoryData>(fullPath);
    }

    void SaveToTwine(Movement movement)
    {
        if (!twineStoryData.unlocked)
        {
            twineStoryData.unlocked = true;
            CustomEvents.RaiseUnlockTwineStory(movement.characterType,twineStoryData);  
            SaveSystem.SaveDataAsync<TwineStoryData>(twineStoryData,fullPath);    
        }
    }

    
    string RemoveFirstUnderscore(string text)
    {
        char[] characters = text.ToCharArray();
        string textWithoutUnderscore = text;

        if (characters[0]=='_')
        {
            text.ToCharArray();

            textWithoutUnderscore = "";

            for (int i = 1; i < characters.Length; i++)
            {
                textWithoutUnderscore = textWithoutUnderscore+characters[i];
            }
        }

        return textWithoutUnderscore;

    }
}
