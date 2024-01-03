using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using Unity.VisualScripting;

/// <summary>
/// Manages the sound system of the game.
/// </summary>
public class SoundSystem : MonoBehaviour
{
    [Header("Background Music Stuff")]
    private static AudioSource backgroundMusicPlayer;
    private static Coroutine backgroundMusicCoroutine;
    private static Coroutine overrideMusicCoroutine;
    private static SoundSystem instance;

    [Header("Default Volumes")]
    [SerializeField] private float SFXDefaultVolume = 1;
    [SerializeField] private float musicDefaultVolume = 0.2f;

    [Header("Audio Source Sound Tasks")]
    private static List<Tuple<AudioSource,Coroutine,int>>  activeSoundTasks = new List<Tuple<AudioSource,Coroutine,Int32>>();
    static Int32 nextId = 0;

    #region Startup
    private void Awake()
    {
        // Set up and play the background music
        //DontDestroyOnLoad(this.gameObject);
        backgroundMusicPlayer = gameObject.AddComponent<AudioSource>();
        backgroundMusicPlayer.loop = true;
        backgroundMusicPlayer.volume = musicDefaultVolume;
    }

    private void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }
    }

    private void OnDisable()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
    #endregion

    private static AudioClip GetAudioClip(string soundName)
    {
        return Resources.Load<AudioClip>(soundName);
    }

    #region PlaySound
    /// <summary>
    /// Plays the specified sound.
    /// </summary>
    /// <param name="soundName">The name of the audioclip to play.</param>
    /// <param name="volume">The volume of the sound (Clamped between 0 and 1).</param>
    public static void PlaySound(string soundName, float delay, float volume = -1)
    {
        AudioClip clip = GetAudioClip(soundName);
        PlaySound(clip, volume, delay,null);
    }

    /// <summary>
    /// Plays the specified sound.
    /// </summary>
    /// <param name="soundName">The name of the audioclip to play.</param>
    /// <param name="volume">The volume of the sound (Clamped between 0 and 1).</param>
    public static void PlaySound(string soundName, float volume)
    {
        AudioClip clip = GetAudioClip(soundName);
        PlaySound(clip, volume,0,null);
    }

    /// <summary>
    /// Plays the specified sound.
    /// </summary>
    /// <param name="soundName">The name of the audioclip to play.</param>
    public static void PlaySound(string soundName)
    {
        AudioClip clip = GetAudioClip(soundName);
        PlaySound(clip,-1,0,null);
    }

    public static void PlaySound(AudioClip soundClip,GameObject audioSourceHolder,float volume = -1)
    {
        PlaySound(soundClip, volume, 0,audioSourceHolder);
    }
    public static void PlaySound(AudioClip soundClip,GameObject audioSourceHolder)
    {
        PlaySound(soundClip, -1, 0,audioSourceHolder);
    }

    public static void PlaySound(AudioClip soundClip, float volume = -1, float delay = 0,GameObject audioSourceHolder=null)
    {
        volume = Mathf.Min(1, volume);

        bool externalAudioSource = false;
    

        AudioSource source=null;
        if (audioSourceHolder!=null && audioSourceHolder.TryGetComponent(out source))
            externalAudioSource = true;

        //Use this object as sourceHolder instead of external one
        if (audioSourceHolder==null)
            audioSourceHolder = instance.gameObject;

        //Create a new AudioSource component to play the sound
        if(source==null)    
            source = audioSourceHolder.gameObject.AddComponent<AudioSource>();

        var soundTask = new Tuple<AudioSource,Coroutine,Int32>(source,null,nextId);

        //Start Coroutine
        IEnumerator i = instance._PlaySound(soundClip,soundTask ,volume, delay, externalAudioSource);
        Coroutine coroutine = instance.StartCoroutine(i);
        
        //Save Soundtask in activeSoundTaksks
        soundTask = new Tuple<AudioSource,Coroutine,Int32>(source,coroutine,nextId);
        nextId++;
        activeSoundTasks.Add(soundTask);
    }   

    
    /// <summary>
    /// Plays a sound with optional volume and delay, and returns a unique task ID.
    /// </summary>
    /// <param name="soundClip">The AudioClip to be played.</param>
    /// <param name="taskId">An out parameter to get the unique task ID.</param>
    /// <param name="volume">The volume of the sound (default is -1, which uses the default volume).</param>
    /// <param name="delay">The delay before playing the sound (default is 0).</param>
    public static void PlaySound(AudioClip soundClip,out Int32 taskId,float volume = -1, float delay = 0)
    {
        taskId = nextId;
        PlaySound(soundClip, volume = -1, delay = 0,null);
    }
    
    private IEnumerator _PlaySound(AudioClip audioClip, Tuple<AudioSource,Coroutine,Int32> audioTask,float volume, float delay,bool externalAudioSource=false)
    {
        //Delay the sound start
        if (delay > 0)
            yield return new WaitForSeconds(delay);

        if (volume <= 0)
            volume = SFXDefaultVolume;

        
        //Set Source Settings And Play
        var source = audioTask.Item1;
        source.volume = volume;
        source.clip = audioClip;
        source.Play();

        // Wait for the sound to finish playing
        yield return new WaitForSeconds(audioClip.length);

        // Destroy the AudioSource component to clean up
        activeSoundTasks.Remove(audioTask);
        if (!externalAudioSource)
            Component.Destroy(source);
    }

    // <summary>
    /// Tries to get an AudioSource by its unique task ID.
    /// </summary>
    /// <param name="id">The unique task ID to search for.</param>
    /// <param name="audioSource">The resulting AudioSource if found.</param>
    /// <returns>True if an AudioSource with the specified ID is found; otherwise, false.</returns>
   static bool TryGetAudioSourceById(Int32 id, out Tuple<AudioSource,Coroutine> soundTaskData)
   {
        for (int i = 0; i < activeSoundTasks.Count; i++)
        {
            var activeSoundTask = activeSoundTasks[i];
            if (activeSoundTask.Item3 == id)
            {
                soundTaskData = new Tuple<AudioSource, Coroutine>(activeSoundTask.Item1,activeSoundTask.Item2);
                return true;
            }    
        }
        soundTaskData = null;
        return false;
   }

    Tuple<AudioSource,Coroutine,Int32> GetAudioTaskById(Int32 id)
   {
        for (int i = 0; i < activeSoundTasks.Count; i++)
        {
            var activeSoundTask = activeSoundTasks[i];
            if (activeSoundTask.Item3 == id)
                return activeSoundTask;
        }
        return null;
   }


   public static bool TryStopSound(int taskId)
   {
        if (TryGetAudioSourceById(taskId, out Tuple<AudioSource,Coroutine> soundTaskData ))
        {
            instance.StopCoroutine(soundTaskData.Item2);
            Component.Destroy(soundTaskData.Item1);
            return true;
        }
        return false;
   }

    #endregion

    #region PlayMusic
    /// <summary>
    /// Plays the specified music and silences the background music.
    /// </summary>
    /// <param name="musicName">The name of the audioclip to play.</param>
    /// <param name="length">For how long the music should play.</param>
    public static void PlayMusic(string musicName, float length = -1, float volume = -1)
    {
        AudioClip clip = GetAudioClip(musicName);
        PlayMusic(clip, length, volume);
    }

    public static void PlayMusic(AudioClip music, float length = -1, float volume = -1)
    {
        volume = Mathf.Min(1, volume);
        if (overrideMusicCoroutine != null)
            instance.StopCoroutine(overrideMusicCoroutine);
        IEnumerator i = instance._PlayMusic(music, length, volume);
        overrideMusicCoroutine = instance.StartCoroutine(i);
    }

    private IEnumerator _PlayMusic(AudioClip clip, float length, float volume)
    {
        float backgroundMusicVolume = backgroundMusicPlayer.volume;
        AudioSource source = gameObject.AddComponent<AudioSource>();
        length = length <= 0 ? clip.length : length;

        backgroundMusicPlayer.volume = 0;
        source.volume = volume <= 0 ? backgroundMusicVolume : volume;
        source.clip = clip;
        source.Play();

        yield return new WaitForSeconds(length);

        UnityEngine.Object.Destroy(source);
        backgroundMusicPlayer.volume = backgroundMusicVolume;

        overrideMusicCoroutine = null;
        yield break;
    }
    #endregion

    #region Background Music
    public static void PlayBackgroundMusic(AudioClip[] musicClips,float fadeTime = 3)
    {
        if (musicClips.Length > 0)
        {
            if (backgroundMusicCoroutine != null)
                instance.StopCoroutine(backgroundMusicCoroutine);
            IEnumerator i = instance._PlayBackgroundMusic(musicClips,fadeTime);
            backgroundMusicCoroutine = instance.StartCoroutine(i);
        }
    }

    private IEnumerator _PlayBackgroundMusic(AudioClip[] musicClips, float fadeTime)
    {
        AudioClip[] backgroundMusicDatabase = musicClips;
        int backgroundMusicPointer = UnityEngine.Random.Range(0,backgroundMusicDatabase.Length);

        while (true)
        {   
            AudioSource source=null;
            if (backgroundMusicPlayer==null)
                source = gameObject.AddComponent<AudioSource>();
            else
                source = backgroundMusicPlayer;
                

            float targetVolume = backgroundMusicPlayer.volume;
            float currentVolume = 0;
            source.volume = fadeTime>0?currentVolume:targetVolume;

            source.clip = backgroundMusicDatabase[backgroundMusicPointer];
            source.Play();

            if (fadeTime>0)
            {
                while (currentVolume < targetVolume)
                {
                    source.volume = currentVolume;
                    backgroundMusicPlayer.volume = targetVolume - currentVolume;

                    currentVolume += targetVolume * Time.deltaTime / fadeTime;
                    yield return null;
                }

                source.volume = targetVolume;

                UnityEngine.Object.Destroy(backgroundMusicPlayer);
                backgroundMusicPlayer = null;
                backgroundMusicPlayer = source;
            }
            else
                source.loop = true;

            yield return new WaitForSeconds(backgroundMusicDatabase[backgroundMusicPointer].length - fadeTime);

            backgroundMusicPointer = (backgroundMusicPointer + 1) % backgroundMusicDatabase.Length;
        }
    }
    #endregion
}