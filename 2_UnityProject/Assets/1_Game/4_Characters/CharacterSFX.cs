using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[System.Serializable]
class FootstepSound
{
    public string tag;
    public E_5_Character[] sounds;
    public bool setAsDefault;
    int lastRandom;

    public E_5_Character GetRandomSound()
    {
        int random = AudioUtility.RandomNumber(lastRandom,sounds.Length,out lastRandom);
        return sounds[random];
    }
}


public class CharacterSFX : MonoBehaviour
{
    [SerializeField]FootstepSound[] footstepSounds;
    List<AudioClip> audioClips = new List<AudioClip>();

    void  Awake()
    {
        AudioUtility audioUtility = new AudioUtility();
        audioUtility.LoadAllAudioClips<E_5_Character>(SaveClips);
    }

    private void SaveClips(AudioClip[] audioClips)
    {
        this.audioClips.AddRange(audioClips);
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

    public void PlayFootstep()
    {   
        //Set up layer mask to ignore character layer
        int characterLayer = LayerMask.NameToLayer("Character");
        LayerMask layerMask = ~(1 << characterLayer);

        //Build Ray
        Ray ray = new Ray(transform.position,Vector3.down);
        RaycastHit hit;
        Physics.Raycast(ray,out hit,Mathf.Infinity,layerMask,QueryTriggerInteraction.Ignore);

        //Get tags
        string[] tags = null;
        if (TryGetComponent(out MultipleTagsTool multipleTagsTool))
            tags = multipleTagsTool.GetTags();

        //Check for matching tags
        if (tags!=null)
        {
            for (int i = 0; i < footstepSounds.Length; i++)
            {
                if (tags.Contains(footstepSounds[i].tag))
                {
                    E_5_Character soundToPlay = footstepSounds[i].GetRandomSound();
                    PlaySound(soundToPlay);
                    return;
                }    
            }
        }


        //Play default footstepsounds in case not matching tag is found
        for (int i = 0; i < footstepSounds.Length; i++)
        {
            if (footstepSounds[i].setAsDefault)
            {
                E_5_Character soundToPlay = footstepSounds[i].GetRandomSound();
                PlaySound(soundToPlay);
                return;
            }    
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
