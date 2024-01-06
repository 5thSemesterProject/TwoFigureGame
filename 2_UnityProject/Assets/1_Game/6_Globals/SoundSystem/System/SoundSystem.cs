using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Runtime.Remoting.Channels;

#region Structs
public enum SoundType
{
    BackgroundMusic = 0,    //EMusic
    Character = 1,          //ECharacter
    VoiceLines = 2,         //EVoicelines
    Ambient = 3,            //EAmbient
    Twine = 4,              //ETwine
}
public enum SoundPriority
{
    None = 0,               //Priority 0
    Default = 1,            //Priority 1
    High = 2,               //Priority 2
    Extreme = 3,            //Priority 3
}

public enum OverrideMode
{
    None,                   //Doesn't cancel previous sounds
    Instant,                //Instantly replaces old
    Crossfade,              //Crossfades between new and old
}

public struct SoundData
{
    public AudioClip soundClip;
    public Transform playTransform;
    public int channel;
    public uint priority;
    public bool loop;
    public float volume;
    public float delay;

    public SoundData(int i)
    {
        soundClip = null;
        playTransform = null;
        channel = default;
        priority = 0;
        loop = false;
        volume = -1;
        delay = -1;
    }
}
#endregion

public enum FadeMode
{
    Default,
    None,
    FadeIn,
    FadeOut,
    FadeInOut,
}

#region SoundChannel
[Serializable]
public class SoundChannel
{
    private List<SoundTask> internalList = new List<SoundTask>();
    [HideInInspector] public bool customChannel = false;
    public bool IsDisposable { get => !customChannel && internalList.Count <= 0; }

    //Values
    public OverrideMode overrideMode = OverrideMode.Instant;
    public FadeMode fadeMode = FadeMode.Default;
    [SerializeField] private float crossfadeTime = 0.5f;
    public float crossfadeDuration { get => overrideMode == OverrideMode.Crossfade ? crossfadeTime : 0; set => crossfadeTime = value; }
    public float volume = 1;
    public float Volume
    {
        get => volume;
        set
        {
            float newVolume = Mathf.Clamp01(value);
            foreach (SoundTask task in internalList)
            {
                task.volume = newVolume * (task.volume / volume);
                task.SyncVolume();
            }
            volume = newVolume;

        }
    }
    public float defaultDelay = 0;
    public bool spatialAudio = false;
    public float defaultMaxDistance = 5;
    public bool checkPriority = false;
    public bool alwaysLoop = false;

    #region List Functionality
    public SoundTask this[int index]
    {
        get { return internalList[index]; }
        set { internalList[index] = value; }
    }
    public IEnumerator GetEnumerator()
    {
        return internalList.GetEnumerator();
    }
    #endregion

    #region Add / Remove
    public bool Add(AudioClip soundClip, Transform playTransform, uint priority = 0, bool loop = false, float volumeMultiplier = -1, FadeMode fadeModeOverride = FadeMode.Default, float fadeDuration = 0.5f, float delay = 0, float maxDistance = -1)
    {
        if (playTransform == null || soundClip == null)
        {
            Debug.LogError("Null reference passed through!");
            return false;
        }

        if (checkPriority)
        {
            if (GetCurrentPriority() >= priority)
                return false;
        }

        //Set Task
        SoundTask task = playTransform.AddComponent<SoundTask>();
        task.clip = soundClip;
        task.channel = this;
        task.spatialize = spatialAudio;
        task.priority = priority;
        task.delay = delay == 0 ? defaultDelay : delay;
        task.volume = volumeMultiplier <= 0 ? volume : volume * volumeMultiplier;
        task.maxDistance = maxDistance <= 0 ? defaultMaxDistance : maxDistance;
        task.fadeDuration = fadeDuration;
        task.fadeMode = fadeModeOverride == FadeMode.Default ? fadeMode : fadeModeOverride;
        task.loop = loop;

        task.Play();

        List<SoundTask> soundTasks = internalList.ToList();
        switch (overrideMode)
        {
            case OverrideMode.None:
                internalList.Add(task);
                break;
            case OverrideMode.Instant:
                foreach (var soundTask in soundTasks)
                {
                    soundTask.Remove();
                }
                internalList.Add(task);
                break;
            case OverrideMode.Crossfade:
                foreach (var soundTask in soundTasks)
                {
                    soundTask.FadeOut(crossfadeDuration);
                }
                internalList.Add(task);
                break;
        }
        return task;
    }
    public void Remove(SoundTask task) => internalList.Remove(task);
    public void RemoveAll()
    {
        foreach (var task in internalList)
            Remove(task);
    }
    public uint GetCurrentPriority()
    {
        if (overrideMode == OverrideMode.None)
            return 0;

        uint priority = 0;
        foreach (var task in internalList)
            priority = task.IsFadingOut ? priority : (priority < task.priority ? task.priority : priority);

        return priority;
    }
    #endregion

    #region Pause
    public void Pause()
    {
        foreach (var task in internalList)
            task.Pause();
    }
    public void Resume()
    {
        foreach (var task in internalList)
            task.Resume();
    }
    #endregion
}
#endregion

#region SoundSystem
public class SoundSystem : MonoBehaviour
{
    //Singleton
    private static SoundSystem instance;

    [Header("Channels")]
    public SerializableDictionary<int, SoundChannel> channels = new SerializableDictionary<int, SoundChannel>();

    #region Startup
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

    #region Pause
    public static void PauseAll()
    {
        foreach (SoundChannel channel in instance.channels)
        {
            channel.Pause();
        }
    }
    public static void ResumeAll()
    {
        foreach (SoundChannel channel in instance.channels)
        {
            channel.Resume();
        }
    }
    public static void PauseChannel(int index)
    {
        SoundChannel channel = GetChannel(index);
        if (channel.IsDisposable)
            RemoveChannel(index);
        else
            channel.Pause();
    }
    public static void ResumeChannel(int index)
    {
        SoundChannel channel = GetChannel(index);
        if (channel.IsDisposable)
            RemoveChannel(index);
        else
            channel.Resume();
    }
    #endregion

    #region Get / Remove / Set
    public static SoundChannel GetChannel(int index)
    {
        SoundChannel channel;
        if (instance.channels.TryGetValue(index, out channel))
            return channel;

        channel = new SoundChannel();
        instance.channels.Add(index, channel);
        return channel;
    }
    public static void RemoveChannel(int index, bool force = false)
    {
        if (GetChannel(index).IsDisposable || force)
            instance.channels.Remove(index);
    }
    public static void SetChannel(int index, SoundChannel channel)
    {
        if (!GetChannel(index).IsDisposable)
            Debug.LogWarning($"Channel {index} was not marked as Disposable");

        instance.channels.ForceAdd(index, channel);
    }
    #endregion

    #region PlaySound
    public static bool PlaySound(AudioClip soundClip, Transform playTransform, int channel, uint priority = 0, bool loop = false, float volume = -1, FadeMode fadeMode = FadeMode.Default, float fadeDuration = 0.5f, float delay = 0, float maxDistance = -1)
    {
        return PlaySound(soundClip, playTransform, GetChannel(channel), priority, loop, volume, fadeMode, fadeDuration, delay, maxDistance);
    }
    private static bool PlaySound(AudioClip soundClip, Transform playTransform, SoundChannel channel, uint priority = 0, bool loop = false, float volume = -1, FadeMode fadeMode = FadeMode.Default, float fadeDuration = 0.5f, float delay = 0, float maxDistance = -1)
    {
        if (soundClip == null)
        {
            Debug.LogError("Soundclip was null!");
            return false;
        }

        //Setup PlayTransfrom
        if (playTransform == null)
        {
            playTransform = instance.transform;
        }

        return channel.Add(soundClip, playTransform, priority, loop, volume, fadeMode, fadeDuration, delay, maxDistance);
    }
    #endregion

    #region Custom
    public static bool Play<T>(T soundClipName, Transform playTransform, SoundPriority priority = 0, bool loop = false, float volume = -1, float delay = 0, FadeMode fadeMode = FadeMode.Default, float fadeDuration = 0f, float maxRange = 5) where T : Enum
    {
        AudioClip clip = SoundHolder.GetAudioClip(soundClipName, out int channel);
        return PlaySound(clip, playTransform, channel, (uint)priority, loop, volume, FadeMode.Default, 0.5f, delay, maxRange);
    }
    #endregion
}
#endregion

#region Serializable Dictionary
[Serializable]
public class SerializableDictionary<TKey, TValue>
{
    private List<TKey> keys = new List<TKey>();

    [SerializeField]
    private List<TValue> values = new List<TValue>();

    private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

    public bool Add(TKey key, TValue value)
    {
        if (!keys.Contains(key))
        {
            keys.Add(key);
            values.Add(value);
            dictionary.Add(key, value);
            return true;
        }
        else
        {
            Debug.LogWarning($"Key '{key}' already exists in the dictionary. Use a different key or update the existing value.");
            return false;
        }
    }
    public void ForceAdd(TKey key, TValue value)
    {
        if (!Add(key, value))
        {
            int index = keys.IndexOf(key);
            values[index] = value;
            dictionary[key] = value;
        }
    }

    public int Count => keys.Count;

    public TKey GetKey(int index)
    {
        if (index >= 0 && index < keys.Count)
        {
            return keys[index];
        }
        else
        {
            Debug.LogError($"Index out of range: {index}");
            return default(TKey);
        }
    }

    public TValue GetValue(int index)
    {
        if (index >= 0 && index < values.Count)
        {
            return values[index];
        }
        else
        {
            Debug.LogError($"Index out of range: {index}");
            return default(TValue);
        }
    }

    public IEnumerator GetEnumerator()
    {
        return values.GetEnumerator();
    }

    public bool Remove(TKey key)
    {
        if (keys.Contains(key))
        {
            int index = keys.IndexOf(key);
            keys.RemoveAt(index);
            values.RemoveAt(index);
            return dictionary.Remove(key);
        }
        else
        {
            Debug.LogWarning($"Key '{key}' not found in the dictionary.");
            return false;
        }
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (dictionary.TryGetValue(key, out value))
        {
            return true;
        }
        else
        {
            value = default(TValue);
            return false;
        }
    }
}
#endregion