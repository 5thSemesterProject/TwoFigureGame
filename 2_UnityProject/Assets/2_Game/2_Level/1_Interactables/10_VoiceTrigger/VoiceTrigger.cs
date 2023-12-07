using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


[CustomEditor(typeof(VoiceTrigger))]
public class MyScriptEditor : Editor
{
  public override void OnInspectorGUI()
  {
    var voiceTrigger = target as VoiceTrigger;

    voiceTrigger.playOnce = EditorGUILayout.Toggle("Play Once", voiceTrigger.playOnce);
    
    voiceTrigger.randomizeVoicelines = EditorGUILayout.Toggle("Randomize Voicelines", voiceTrigger.randomizeVoicelines);

    voiceTrigger.extraWaitTimeBetweenClips = EditorGUILayout.FloatField("Wait Time Between Clips", voiceTrigger.extraWaitTimeBetweenClips);

    voiceTrigger.triggerType = (TriggerType)EditorGUILayout.EnumPopup("TriggerType",voiceTrigger.triggerType);

    if (voiceTrigger.randomizeVoicelines)
        EditorGUILayout.PropertyField(serializedObject.FindProperty("randomVoicelines"),true);
    else
        voiceTrigger.voiceLine = (E_1_Voicelines)EditorGUILayout.EnumPopup("Voiceline",voiceTrigger.voiceLine);

    serializedObject.ApplyModifiedProperties();


  }
}



[RequireComponent(typeof(Interactable))]
public class VoiceTrigger : MonoBehaviour
{
    Interactable interactable;
    AudioClip voiceClip;
    bool triggered = false;
    Coroutine coroutine;
    int lastRandom = 500;
    
    public float extraWaitTimeBetweenClips = 1f;
    public bool playOnce = false;
    public E_1_Voicelines voiceLine;
    public bool randomizeVoicelines;
    public E_1_Voicelines[] randomVoicelines;

    public TriggerType triggerType;



    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<Interactable>();

        if (triggerType== TriggerType.OnTrigger)
        {
            interactable.triggerEvent+=LoadVoiceLine;
            interactable.untriggerEvent+=Untrigger;
        }
        else if (triggerType == TriggerType.OnHighlight)
        {
            interactable.highlightEvent+=LoadVoiceLine;
            interactable.unhiglightEvent+=Untrigger;
        }
            
    }

    void LoadVoiceLine(Movement movement)
    {
        if (coroutine==null && !triggered)
        {
            triggered = true;

            E_1_Voicelines voicelineToPlay; 
            
            if (randomizeVoicelines)
                voicelineToPlay = RandomVoiceLine();
            else  
                voicelineToPlay = voiceLine;

            string fileName = Enum.GetName(typeof(E_1_Voicelines),voicelineToPlay);
            fileName = RemoveFirstUnderscore(fileName);
            AsyncOperationHandle<AudioClip> asyncOperationHandle =  Addressables.LoadAssetAsync<AudioClip>("Assets/4_Assets/2_Sound/1_Voicelines/"+fileName+".wav");
            asyncOperationHandle.Completed+=PlayVoiceLine;
        }
    }

    void PlayVoiceLine(AsyncOperationHandle<AudioClip> asyncOperationHandle)
    {
        voiceClip = asyncOperationHandle.Result;
        coroutine  = StartCoroutine(_PlayVoiceLine());
    }

    IEnumerator _PlayVoiceLine()
    {
        SoundSystem.PlaySound(voiceClip);
        yield return new WaitForSeconds(voiceClip.length+extraWaitTimeBetweenClips);

        if (!playOnce)
        {
            coroutine = null;
        }

    }

    E_1_Voicelines RandomVoiceLine()
    {
        int random = lastRandom;
        while(random==lastRandom)
            random = UnityEngine.Random.Range(0,randomVoicelines.Length-1);
        lastRandom = random;
        return randomVoicelines[random];
    }
    

    void Untrigger(Movement movement)
    {
        triggered = false;
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
