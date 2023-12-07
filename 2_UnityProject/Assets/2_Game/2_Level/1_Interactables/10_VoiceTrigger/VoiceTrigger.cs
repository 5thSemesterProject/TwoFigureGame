using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


[RequireComponent(typeof(Interactable))]
public class VoiceTrigger : MonoBehaviour
{
    Interactable interactable;
    AudioClip voiceLine;
    [SerializeField] E_1_Voicelines e_1_Voicelines;

    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<Interactable>();
        interactable.triggerEvent+=LoadVoiceLine;
        interactable.unhiglightEvent+=ClearVoiceLine;
    }

    void LoadVoiceLine(Movement movement)
    {
        if (voiceLine==null)
        {
            string fileName = Enum.GetName(typeof(E_1_Voicelines),e_1_Voicelines);
            fileName = RemoveFirstUnderscore(fileName);
            Debug.Log (fileName);
            AsyncOperationHandle<AudioClip> asyncOperationHandle =  Addressables.LoadAssetAsync<AudioClip>("Assets/4_Assets/2_Sound/1_Voicelines/"+fileName+".wav");
            asyncOperationHandle.Completed+=PlayVoiceLine;
        }
    }

    void PlayVoiceLine(AsyncOperationHandle<AudioClip> asyncOperationHandle)
    {
        voiceLine = asyncOperationHandle.Result;
        StartCoroutine(_PlayVoiceLine());
    }

    IEnumerator _PlayVoiceLine()
    {
        SoundSystem.PlaySound(voiceLine);
        yield return new WaitForSeconds(voiceLine.length);
    }

    void ClearVoiceLine(Movement movement)
    {
        voiceLine = null;
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
