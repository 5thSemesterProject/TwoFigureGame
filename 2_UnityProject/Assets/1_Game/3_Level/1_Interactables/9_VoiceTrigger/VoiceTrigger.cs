using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


[CustomEditor(typeof(VoiceTrigger))]
public class MyScriptEditor : Editor
{
    private const string PlayOnceKey = "VoiceTrigger_PlayOnce";
    private const string PlayOnceBeforeRandomKey = "VoiceTrigger_PlayOnceBeforeRandom";
    private const string VoiceLineKey = "VoiceTrigger_VoiceLine";

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var voiceTrigger = (VoiceTrigger)target;

        voiceTrigger.playOnce = EditorPrefs.GetBool(PlayOnceKey, voiceTrigger.playOnce);
        voiceTrigger.playOnceBeforeRandom = EditorPrefs.GetBool(PlayOnceBeforeRandomKey, voiceTrigger.playOnceBeforeRandom);
        voiceTrigger.voiceLine = (E_1_Voicelines)EditorPrefs.GetInt(VoiceLineKey, (int)voiceTrigger.voiceLine);

        voiceTrigger.playOnce = EditorGUILayout.Toggle("Play Once", voiceTrigger.playOnce);

        voiceTrigger.randomizeVoicelines = EditorGUILayout.Toggle("Randomize Voicelines", voiceTrigger.randomizeVoicelines);

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

        voiceTrigger.extraWaitTimeAfterClip = EditorGUILayout.FloatField("Wait Time Between Clips", voiceTrigger.extraWaitTimeAfterClip);

        voiceTrigger.triggerType = (TriggerType)EditorGUILayout.EnumPopup("TriggerType",voiceTrigger.triggerType);

        if (voiceTrigger.randomizeVoicelines)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("randomVoicelines"),true);
            voiceTrigger.playOnceBeforeRandom = EditorGUILayout.Toggle("Play Once Before Random", voiceTrigger.playOnceBeforeRandom);
        }    
        else
            voiceTrigger.voiceLine = (E_1_Voicelines)EditorGUILayout.EnumPopup("Voiceline",voiceTrigger.voiceLine);

        if (voiceTrigger.playOnceBeforeRandom)
            voiceTrigger.voiceLine = (E_1_Voicelines)EditorGUILayout.EnumPopup("Voiceline before Random",voiceTrigger.voiceLine);

        EditorPrefs.SetBool(PlayOnceKey, voiceTrigger.playOnce);
        EditorPrefs.SetBool(PlayOnceBeforeRandomKey, voiceTrigger.playOnceBeforeRandom);
        EditorPrefs.SetInt(VoiceLineKey, (int)voiceTrigger.voiceLine);

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
    public bool playOnce;
    public bool playOnceBeforeRandom;
    public E_1_Voicelines voiceLine;
    public bool randomizeVoicelines;
    public E_1_Voicelines[] randomVoicelines;
    public TriggerType triggerType;

    public CharacterType characterType = CharacterType.None;

    private void Awake()
    {
        Debug.Log("Awake - playOnce: " + playOnce + ", playOnceBeforeRandom: " + playOnceBeforeRandom + ", voiceLine: " + voiceLine);
    }

    private void OnEnable()
    {
        Debug.Log("OnEnable - playOnce: " + playOnce + ", playOnceBeforeRandom: " + playOnceBeforeRandom + ", voiceLine: " + voiceLine);
    }

    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<Interactable>();

        if (triggerType== TriggerType.OnTrigger)
        {
            interactable.triggerEvent += LoadVoiceLine;

            if (!playOnce)
                interactable.untriggerEvent += Untrigger;
        }
        else if (triggerType == TriggerType.OnHighlight)
        {
            interactable.highlightEvent += LoadVoiceLine;

            if (!playOnce)
                interactable.unhiglightEvent += Untrigger;
        }

        AddOtherVoiceTriggerRange(GetComponents<VoiceTrigger>());

    }

    void LoadVoiceLine(Movement movement)
    {
        if (coroutine==null && !triggered 
        && CheckCharacterTypes(movement)
        && CheckOtherVoicelines())
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

            Debug.Log(randomVoicelines.Length);
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
        if (random==lastRandom)
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


