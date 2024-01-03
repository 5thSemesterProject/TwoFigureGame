using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using Unity.IO.LowLevel.Unsafe;
using System.Linq;

public class VoicelinePlayer : MonoBehaviour
{
    private Coroutine coroutine;
    private float extraWaitTimeAfterClip;
    public static VoicelinePlayer instance;
    static int activeTaskPriority;
    static Int32 activeTaskId;
    static List<AudioClip> voicelines = new List<AudioClip>();

    void Awake()
    {
        //DontDestroyOnLoad(gameObject);

        //Singleton Steup    
        if (instance==null)
            instance = this;
        else
            Destroy(this);

        //Load All Voicelines
        AudioUtility audioUtility = new AudioUtility();
        audioUtility.LoadAllAudioClipsAsync<E_1_Voicelines>(SaveVoiceLines);
    }

    void SaveVoiceLines(AudioClip[] audioClips)
    {
        voicelines.AddRange(audioClips);
    }


    public bool TryPlayVoiceLine(E_1_Voicelines voiceline,float extraWaitTimeAfterClip=0,int priority=1)
    {
        string fileName = Enum.GetName(typeof(E_1_Voicelines),voiceline);
        fileName = AudioUtility.RemovePrefix(fileName,"_");
        return TryPlayVoiceLine(fileName,extraWaitTimeAfterClip,priority);
    }

    public bool TryPlayVoiceLine(string fileName,float extraWaitTimeAfterClip=0,int priority=1)
    {
        if (coroutine==null)
        {
            activeTaskPriority = priority;
            AudioClip voiceClip = voicelines.FirstOrDefault(clip=>clip.name==fileName);
            if (voiceClip!=null)
            {
                coroutine  = StartCoroutine(PlayVoiceLine(voiceClip));
                this.extraWaitTimeAfterClip = extraWaitTimeAfterClip;
                return true;
            }
            return false;
        }
        else if (priority>activeTaskPriority)
        {
            StopCoroutine(coroutine);
            coroutine = null;
            SoundSystem.TryStopSound(activeTaskId);
            return TryPlayVoiceLine(fileName,extraWaitTimeAfterClip,priority);
        }
        return false;
    }


    IEnumerator PlayVoiceLine(AudioClip voiceClip)
    {
        SoundSystem.PlaySound(voiceClip,out activeTaskId);
        yield return new WaitForSeconds(voiceClip.length+extraWaitTimeAfterClip);
        coroutine = null;
    }

}
