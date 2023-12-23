using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public enum CharacterMode
{
    Both,Active, Inactive
}


[CustomEditor(typeof(VoiceTrigger))]
public class MyScriptEditor : Editor
{
  public override void OnInspectorGUI()
  {
    var voiceTrigger = target as VoiceTrigger;

    EditorGUILayout.LabelField("Trigger Conditions", EditorStyles.boldLabel);

    voiceTrigger.playOnce = EditorGUILayout.Toggle("Play Once", voiceTrigger.playOnce);
    
    //Gender Specific Voices
    Interactable interactable = voiceTrigger.GetComponent<Interactable>();
    if (interactable && interactable.specificCharacterAccess==CharacterType.None)
    {
        voiceTrigger.characterType = (CharacterType)EditorGUILayout.EnumPopup("Character",voiceTrigger.characterType);
    }
    else
    {
        voiceTrigger.characterType = CharacterType.None;
    }

    voiceTrigger.triggerType = (TriggerType)EditorGUILayout.EnumPopup("TriggerType",voiceTrigger.triggerType);

    voiceTrigger.characterMode = (CharacterMode)EditorGUILayout.EnumPopup("Required Character Mode",voiceTrigger.characterMode);

    EditorGUILayout.Space(10);
    EditorGUILayout.LabelField("Voiceline Settings", EditorStyles.boldLabel);

    voiceTrigger.extraWaitTimeAfterClip = EditorGUILayout.FloatField("Wait Time Between Clips", voiceTrigger.extraWaitTimeAfterClip);

    voiceTrigger.randomizeVoicelines = EditorGUILayout.Toggle("Randomize Voicelines", voiceTrigger.randomizeVoicelines);

    if (voiceTrigger.randomizeVoicelines)
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("randomVoicelines"),true);
        voiceTrigger.playOnceBeforeRandom = EditorGUILayout.Toggle("Play Once Before Random", voiceTrigger.playOnceBeforeRandom);
    }    
    else
        voiceTrigger.voiceLine = (E_1_Voicelines)EditorGUILayout.EnumPopup("Voiceline",voiceTrigger.voiceLine);

    if (voiceTrigger.playOnceBeforeRandom)
        voiceTrigger.voiceLine = (E_1_Voicelines)EditorGUILayout.EnumPopup("Voiceline before Random",voiceTrigger.voiceLine);

    serializedObject.ApplyModifiedProperties();


  }
}



[RequireComponent(typeof(Interactable))]
public class VoiceTrigger : MonoBehaviour
{
    Interactable interactable;
    AudioClip voiceClip;
    bool triggered = false;

    int lastRandom = 500;

    static List<VoiceTrigger>otherVoiceTriggers = new List<VoiceTrigger>();
    public Coroutine coroutine;
    public float extraWaitTimeAfterClip = 1f;
    public bool playOnce = false;
    public bool playOnceBeforeRandom;
    public E_1_Voicelines voiceLine;
    public bool randomizeVoicelines;
    public E_1_Voicelines[] randomVoicelines;
    public TriggerType triggerType;
    public CharacterType characterType = CharacterType.None;
    public CharacterMode characterMode = CharacterMode.Both;


    void Start()
    {
        interactable = GetComponent<Interactable>();

        if (triggerType== TriggerType.OnTrigger)
        {

            interactable.triggerEvent+=LoadVoiceLine;

            if (!playOnce)
                interactable.untriggerEvent+=Untrigger;
        }
        else if (triggerType == TriggerType.OnHighlight)
        {
            if (characterMode != CharacterMode.Inactive)
            {
                interactable.highlightEvent+=LoadVoiceLine;
            
                if (!playOnce)
                    interactable.unhiglightEvent+=Untrigger;
            }
            else
            {
                interactable.aiEnterEvent+=LoadVoiceLine;
                interactable.aiStayEvent += LoadVoiceLine;
                
                if (!playOnce)
                    interactable.aiExitEvent+=Untrigger;
            }   

        }

        AddOtherVoiceTriggerRange(GetComponents<VoiceTrigger>());

    }

    void LoadVoiceLine(Movement movement)
    {
        if (coroutine==null && !triggered 
        && CheckCharacterTypes(movement)
        && CheckOtherVoicelines()
        && CheckCharacterMode(movement))
        {
            triggered = true;

            E_1_Voicelines voicelineToPlay; 
            
            if (randomizeVoicelines)
                voicelineToPlay = RandomVoiceLine();
            else  
                voicelineToPlay = voiceLine;

            if (playOnceBeforeRandom)
            {   
                playOnceBeforeRandom = false;
                voicelineToPlay = voiceLine;
            }    

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
        yield return new WaitForSeconds(voiceClip.length+extraWaitTimeAfterClip);

        coroutine = null;
    }

    E_1_Voicelines RandomVoiceLine()
    {
        int random = UnityEngine.Random.Range(0,randomVoicelines.Length-1);
        if(random==lastRandom)
            random = (random + 1) % randomVoicelines.Length;
        lastRandom = random;
        return randomVoicelines[random];
    }
    

    void Untrigger(Movement movement)
    {
        if (CheckCharacterTypes(movement))
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

    bool CheckOtherVoicelines()
    {
        for (int i = 0; i < otherVoiceTriggers.Count; i++)
        {
            if (otherVoiceTriggers[i].coroutine !=null)
                return false;
        }
        return true;
    }

    bool CheckCharacterTypes(Movement movement)
    {
        return interactable.specificCharacterAccess == CharacterType.None && movement.characterType == characterType && characterType!=CharacterType.None
         ||interactable.specificCharacterAccess!= CharacterType.None
         ||characterType==CharacterType.None;
    }

    bool CheckCharacterMode(Movement movement)
    {
        Movement activeMovement = CharacterManager.ActiveCharacterData.movement;

        switch(characterMode)
        {
            case CharacterMode.Both:
                return true;
            case CharacterMode.Active:
                if (movement == activeMovement)
                    return true;
                break;
            case CharacterMode.Inactive:
                if (movement != activeMovement)
                    return true;
                break;
        }
        return false;
    }

    void AddOtherVoiceTriggerRange(VoiceTrigger[] voiceTriggers)
    {
        for (int i = 0; i < voiceTriggers.Length; i++)
        {
            TryAddOtherVoiceTrigger(voiceTriggers[i]);
        }
    }

    bool TryAddOtherVoiceTrigger(VoiceTrigger voiceTrigger)
    {
        if (otherVoiceTriggers.Contains(voiceTrigger))
            return false;

        otherVoiceTriggers.Add(voiceTrigger);
        return true;
    }
}


