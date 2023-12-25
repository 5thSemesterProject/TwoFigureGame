using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

public class VoicelinePlayer : MonoBehaviour
{
    private Coroutine coroutine;
    private float extraWaitTimeAfterClip;
    public static VoicelinePlayer instance;

    void Awake()
    {
        if (instance==null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public bool LoadAndPlayVoiceLine(E_1_Voicelines voiceline,float extraWaitTimeAfterClip)
    {
        string fileName = Enum.GetName(typeof(E_1_Voicelines),voiceline);
        fileName = RemoveFirstUnderscore(fileName);
        return LoadVoiceLine(fileName,extraWaitTimeAfterClip);
    }

    bool LoadVoiceLine(string fileName,float extraWaitTimeAfterClip=0)
    {
        if (coroutine==null)
        {
            AsyncOperationHandle<AudioClip> asyncOperationHandle =  Addressables.LoadAssetAsync<AudioClip>("Assets/4_Assets/2_Sound/1_Voicelines/"+fileName+".wav");
            asyncOperationHandle.Completed+=PlayVoiceLine;
            this.extraWaitTimeAfterClip = extraWaitTimeAfterClip;
            return true;
        }
        return false;
    }

    void PlayVoiceLine(AsyncOperationHandle<AudioClip> asyncOperationHandle)
    {
        AudioClip voiceClip = asyncOperationHandle.Result;
        coroutine  = StartCoroutine(_PlayVoiceLine(voiceClip));
    }

    IEnumerator _PlayVoiceLine(AudioClip voiceClip)
    {
        SoundSystem.PlaySound(voiceClip);
        yield return new WaitForSeconds(voiceClip.length+extraWaitTimeAfterClip);
        coroutine = null;
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
