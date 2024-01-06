using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPlaySound : CustomButtonFunctionality
{
    [SerializeField] private ButtonEventType triggerMoment = ButtonEventType.Click;

    [SerializeField] private AudioClip clip;
    [SerializeField] private float volume = 1;
    [SerializeField] private FadeMode fadeMode = FadeMode.None;
    [SerializeField] private float fadeDuration = 0f;

    protected override void OnAwake()
    {
        AddFunctionToEvent(PlayClip, triggerMoment);
    }

    private void PlayClip()
    {
        SoundSystem.PlaySound(clip, null, 1, 0, false, volume, fadeMode, fadeDuration);
    }
}
