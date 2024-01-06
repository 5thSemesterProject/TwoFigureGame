using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundDebugger : MonoBehaviour
{
    [SerializeField] private AudioClip AudioClipToPlay;
    [SerializeField] private int channel;
    [SerializeField] private SoundPriority priority = SoundPriority.None;
    [SerializeField] private bool loop = false;
    [SerializeField] private float volume = 1;
    [SerializeField] private FadeMode fadeMode = FadeMode.Default;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float delay = 0;
    [SerializeField] private float maxDistance = 5;

    private void OnGUI()
    {
        if (GUILayout.Button("PlaySound"))
            PlaySound();
        if (GUILayout.Button("PauseSound"))
            SoundSystem.PauseAll();
        if (GUILayout.Button("ResumeSounds"))
            SoundSystem.ResumeAll();
    }

    private void PlaySound()
    {
        Debug.Log(SoundSystem.PlaySound(AudioClipToPlay, this.transform, channel, (uint) priority,loop, volume, fadeMode, fadeDuration, delay, maxDistance));
    }
}
