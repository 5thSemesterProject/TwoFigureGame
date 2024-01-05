using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
class FootstepSound
{
    public string tag;
    public ECharacterSounds[] sounds;
    public bool setAsDefault;
    int lastRandom;

    public ECharacterSounds GetRandomSound()
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
        audioUtility.LoadAllAudioClips<ECharacterSounds>(SaveClips);
    }

    private void SaveClips(AudioClip[] audioClips)
    {
        this.audioClips.AddRange(audioClips);
    }

    public void PlaySound(ECharacterSounds soundEffectToPlay)
    {
        PlaySoundWithVolumeControl(soundEffectToPlay,-1);
    }

    public void PlaySoundWithVolumeControl(ECharacterSounds soundEffectToPlay,float volume=-1)
    {
        string fileName = Enum.GetName(typeof(ECharacterSounds),soundEffectToPlay);
        
        for (int i = 0; i < audioClips.Count; i++)
        {
            if (fileName == audioClips[i].name)
                SoundSystem.Play(soundEffectToPlay, gameObject.transform, SoundPriority.None, false, volume);
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
                    ECharacterSounds soundToPlay = footstepSounds[i].GetRandomSound();
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
                ECharacterSounds soundToPlay = footstepSounds[i].GetRandomSound();
                PlaySoundWithVolumeControl(soundToPlay,volume);
                return;
            }    
        }
    }
}
