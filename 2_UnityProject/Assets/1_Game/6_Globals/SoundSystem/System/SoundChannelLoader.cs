using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundChannelLoader : MonoBehaviour
{
    [SerializeField] private int channelNumber;
    [SerializeField] private SoundChannel channel = new SoundChannel();
    [SerializeField] private bool loadIntoSoundSystem = true;
    private bool canLoad = false;

    private void OnValidate()
    {
        channel.customChannel = true;

        if (Application.isPlaying && loadIntoSoundSystem && canLoad)
            SoundSystem.SetChannel(channelNumber, channel);
    }

    private void Start()
    {
        channel.customChannel = true;
        canLoad = true;

        if (loadIntoSoundSystem)
            SoundSystem.SetChannel(channelNumber, channel);
    }
}
