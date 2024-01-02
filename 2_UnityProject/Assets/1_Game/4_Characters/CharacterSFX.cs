using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
        PlaySoundWithVolumeControl(soundEffectToPlay,-1);
    }

    public void PlaySoundWithVolumeControl(E_5_Character soundEffectToPlay,float volume=-1)
    {
        string fileName = Enum.GetName(typeof(E_5_Character),soundEffectToPlay);
        
        for (int i = 0; i < audioClips.Count; i++)
        {
            if (fileName==audioClips[i].name)
                SoundSystem.PlaySound(audioClips[i],gameObject,volume);
        }
    }

    public void PlayFootstep()
    {
        PlayFootstepWithVolumeControl(-1);
    }

    public void PlayFootstepWithVolumeControl(float volume = -1)
    {   
        //Set up layer mask to ignore character layer
        int characterLayer = LayerMask.NameToLayer("Player");
        LayerMask layerMask = ~(1 << characterLayer);

        //Build  and cast Ray
        Ray ray = new Ray(transform.position+Vector3.up,Vector3.down);
        RaycastHit hit;
        Physics.Raycast(ray,out hit,Mathf.Infinity,layerMask);
        
        Debug.DrawRay(transform.position+Vector3.up,Vector3.down*200, Color.red,10000);

        //Get tags
        string[] tags = null;
        if (hit.transform!=null)
        {
            var multipleTagsTool = hit.transform.GetComponent<MultipleTagsTool>();
            if (multipleTagsTool!=null)
            {
                tags = multipleTagsTool.GetTags();
            }
        }   

        //Check for matching tags
        if (tags!=null)
        {
            for (int i = 0; i < footstepSounds.Length; i++)
            {
                if (tags.Contains(footstepSounds[i].tag))
                {
                    E_5_Character soundToPlay = footstepSounds[i].GetRandomSound();
                    PlaySoundWithVolumeControl(soundToPlay,volume);
                    return;
                }    
            }
        }


        //Play default footstepsounds in case not matching tag is found
        for (int i = 0; i < footstepSounds.Length; i++)
        {
            if (footstepSounds[i].setAsDefault)
            {   
               // Debug.Log ("Default");
                E_5_Character soundToPlay = footstepSounds[i].GetRandomSound();
                PlaySoundWithVolumeControl(soundToPlay,volume);
                return;
            }    
        }
    }
}
