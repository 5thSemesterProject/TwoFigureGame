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

#if UNITY_EDITOR
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

    voiceTrigger.priority = (SoundPriority)EditorGUILayout.EnumPopup("Priority",voiceTrigger.priority);

    voiceTrigger.characterMode = (CharacterMode)EditorGUILayout.EnumPopup("Required Character Mode",voiceTrigger.characterMode);

    EditorGUILayout.Space(10);
    EditorGUILayout.LabelField("Voiceline Settings", EditorStyles.boldLabel);

    voiceTrigger.volume = EditorGUILayout.FloatField("Volume", voiceTrigger.volume);

    voiceTrigger.extraWaitTimeAfterClip = EditorGUILayout.FloatField("Wait Time Between Clips", voiceTrigger.extraWaitTimeAfterClip);

    voiceTrigger.randomizeVoicelines = EditorGUILayout.Toggle("Randomize Voicelines", voiceTrigger.randomizeVoicelines);

    if (voiceTrigger.randomizeVoicelines)
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("randomVoicelines"),true);
        voiceTrigger.playOnceBeforeRandom = EditorGUILayout.Toggle("Play Once Before Random", voiceTrigger.playOnceBeforeRandom);
    }    
    else
        voiceTrigger.voiceLine = (EVoicelines)EditorGUILayout.EnumPopup("Voiceline",voiceTrigger.voiceLine);

    if (voiceTrigger.playOnceBeforeRandom)
        voiceTrigger.voiceLine = (EVoicelines)EditorGUILayout.EnumPopup("Voiceline before Random",voiceTrigger.voiceLine);

    serializedObject.ApplyModifiedProperties();


  }
}
#endif


[RequireComponent(typeof(Interactable))]
public class VoiceTrigger : MonoBehaviour
{
    Interactable interactable;
    bool triggered = false;

    int lastRandom = 500;

    public float volume = 1;
    public float extraWaitTimeAfterClip = 1f;
    public bool playOnce = false;
    public bool playOnceBeforeRandom;
    public EVoicelines voiceLine;
    public bool randomizeVoicelines;
    public EVoicelines[] randomVoicelines;
    public TriggerType triggerType;
    public CharacterType characterType = CharacterType.None;
    public CharacterMode characterMode = CharacterMode.Both;
    public SoundPriority priority = SoundPriority.Default;


    void Start()
    {
        interactable = GetComponent<Interactable>();

        switch (triggerType)
        {
            case TriggerType.OnHighlight:
                if (characterMode != CharacterMode.Inactive)
                {
                    interactable.highlightEvent += LoadVoiceLine;

                    if (!playOnce)
                        interactable.unhiglightEvent += Untrigger;
                }
                else
                {
                    interactable.aiEnterEvent += LoadVoiceLine;
                    interactable.aiStayEvent += LoadVoiceLine;

                    if (!playOnce)
                        interactable.aiExitEvent += Untrigger;
                }
                break;
            case TriggerType.OnTrigger:
                interactable.triggerEvent += LoadVoiceLine;

                if (!playOnce)
                    interactable.untriggerEvent += Untrigger;
                break;
            case TriggerType.OnUntrigger:
                interactable.untriggerEvent += LoadVoiceLine;

                if (!playOnce)
                    interactable.triggerEvent += Untrigger;
                break;
            case TriggerType.OnUnHighlight:
                if (characterMode != CharacterMode.Inactive)
                {
                    interactable.unhiglightEvent += LoadVoiceLine;

                    if (!playOnce)
                        interactable.highlightEvent += Untrigger;
                }
                else
                {
                    interactable.aiExitEvent += LoadVoiceLine;

                    if (!playOnce)
                        interactable.aiEnterEvent += Untrigger;
                }
                break;
            default:
                break;
        }
    }

    void LoadVoiceLine(Movement movement)
    {
        if (!triggered 
        && CheckCharacterTypes(movement)
        && CheckCharacterMode(movement))
        {
            triggered = true;

            EVoicelines voicelineToPlay; 
            
            if (randomizeVoicelines)
                voicelineToPlay = randomVoicelines[AudioUtility.RandomNumber(lastRandom,randomVoicelines.Length,out lastRandom)];
            else  
                voicelineToPlay = voiceLine;

            if (playOnceBeforeRandom)
            {   
                playOnceBeforeRandom = false;
                voicelineToPlay = voiceLine;
            }

            SoundSystem.Play(voicelineToPlay, null, priority, false, volume, -extraWaitTimeAfterClip);
        }
    }


    void Untrigger(Movement movement)
    {
        if (CheckCharacterTypes(movement))
            triggered = false;
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

}


