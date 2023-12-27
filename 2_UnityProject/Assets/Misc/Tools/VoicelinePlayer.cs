using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using Unity.IO.LowLevel.Unsafe;

public class VoicelinePlayer : MonoBehaviour
{
    private Coroutine coroutine;
    private float extraWaitTimeAfterClip;
    public static VoicelinePlayer instance;
    static int activeTaskPriority;
    static Int32 activeTaskId;

    void Awake()
    {
        AsyncOperationHandle<AudioClip> asyncOperationHandle =  Addressables.LoadAssetAsync<AudioClip>("Assets/4_Assets/2_Sound/1_Voicelines/"+"Box_E_01"+".wav");

        if (instance==null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public bool LoadAndPlayVoiceLine(E_1_Voicelines voiceline,float extraWaitTimeAfterClip,int priority=1)
    {
        string fileName = Enum.GetName(typeof(E_1_Voicelines),voiceline);
        fileName = RemoveFirstUnderscore(fileName);
        return LoadVoiceLine(fileName,extraWaitTimeAfterClip,priority);
    }

    bool LoadVoiceLine(string fileName,float extraWaitTimeAfterClip=0,int priority=1)
    {
        if (coroutine==null)
        {
            activeTaskPriority = priority;
            AsyncOperationHandle<AudioClip> asyncOperationHandle =  Addressables.LoadAssetAsync<AudioClip>("Assets/4_Assets/2_Sound/1_Voicelines/"+fileName+".wav");
            asyncOperationHandle.Completed+=PlayVoiceLine;
            this.extraWaitTimeAfterClip = extraWaitTimeAfterClip;
            return true;
        }
        else if (priority>activeTaskPriority)
        {
            StopCoroutine(coroutine);
            coroutine = null;
            SoundSystem.TryStopSound(activeTaskId);
            return LoadVoiceLine(fileName,extraWaitTimeAfterClip,priority);
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
        SoundSystem.PlaySound(voiceClip,out activeTaskId);
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
