using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CharacterSFX : MonoBehaviour
{
    List<AudioClip> audioClips = new List<AudioClip>();

    void  Awake()
    {
        LoadAllAudioClips();
    }

    private void LoadAllAudioClips()
    {
        string[] filenames = Enum.GetNames(typeof(E_5_Character));

        for (int i = 0; i < filenames.Length; i++)
        {
            filenames[i] = RemoveFirstUnderscore(filenames[i]);
            AsyncOperationHandle<AudioClip> asyncOperationHandle =  Addressables.LoadAssetAsync<AudioClip>($"Assets/2_Resources/2_Sound/5_Character/{filenames[i]}.wav");
            asyncOperationHandle.Completed+=LoadClipToMemory;
        }
    }

    void LoadClipToMemory(AsyncOperationHandle<AudioClip> asyncOperationHandle)
    {
        AudioClip audioClip = asyncOperationHandle.Result;
        audioClips.Add(audioClip);
    }


    public void PlaySound(E_5_Character soundEffectToPlay)
    {
        string fileName = Enum.GetName(typeof(E_5_Character),soundEffectToPlay);
        
        for (int i = 0; i < audioClips.Count; i++)
        {
            if (fileName==audioClips[i].name)
                SoundSystem.PlaySound(audioClips[i],gameObject);
        }
    }

    private string RemoveFirstUnderscore(string text)
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
