using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoundHolder : MonoBehaviour
{
    private static List<AudioClip> voicelines = new List<AudioClip>();
    private static List<AudioClip> ambient = new List<AudioClip>();
    private static List<AudioClip> character = new List<AudioClip>();
    private static List<AudioClip> twine = new List<AudioClip>();
    private static List<AudioClip> backgroundMusic = new List<AudioClip>();
    private static bool clipsLoaded = false;

    private void Awake()
    {
        LoadAudioClips();
    }

    private static void LoadAudioClips(bool force = false)
    {
        if (clipsLoaded && !force)
            return;
        AudioUtility audioUtility = new AudioUtility();
        audioUtility.LoadAllAudioClips<EVoicelines>((AudioClip[] audioClips) => voicelines = audioClips.ToList());
        audioUtility = new AudioUtility();
        audioUtility.LoadAllAudioClips<EAmbientSounds>((AudioClip[] audioClips) => ambient = audioClips.ToList());
        audioUtility = new AudioUtility();
        audioUtility.LoadAllAudioClips<ECharacterSounds>((AudioClip[] audioClips) => character = audioClips.ToList());
        audioUtility = new AudioUtility();
        audioUtility.LoadAllAudioClips<EMusic>((AudioClip[] audioClips) => backgroundMusic = audioClips.ToList());
        clipsLoaded = true;
    }

    public static AudioClip GetAudioClip<T>(T soundEnum, out int channel) where T : Enum
    {
        string name = Enum.GetName(typeof(T), soundEnum);
        name = AudioUtility.RemovePrefix(name, "_");
        AudioClip returnClip = null;

        channel = 3;
        if (typeof(T) == typeof(EVoicelines))
        {
            channel = 2;
            returnClip = voicelines.FirstOrDefault(clip => clip.name == name);
        }
        else if (typeof(T) == typeof(EAmbientSounds))
        {
            channel = 3;
            returnClip = ambient.FirstOrDefault(clip => clip.name == name);
        }
        else if (typeof(T) == typeof(ECharacterSounds))
        {
            channel = 3;
            returnClip = character.FirstOrDefault(clip => clip.name == name);
        }
        else if (typeof(T) == typeof(EMusic))
        {
            channel = 0;
            returnClip = backgroundMusic.FirstOrDefault(clip => clip.name == name);
        }

        if (returnClip == null)
        {
            LoadAudioClips(true);
        }
        return returnClip;
    }
}
