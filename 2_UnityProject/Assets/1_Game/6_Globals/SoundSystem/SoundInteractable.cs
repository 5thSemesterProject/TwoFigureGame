using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class SoundInteractable : MonoBehaviour
{
    private Interactable interactable;
    [SerializeField] private TriggerType triggerType = TriggerType.OnTrigger;
    [SerializeField] private EAmbientSounds sound;
    [SerializeField] private SoundPriority soundPriority = SoundPriority.None;
    [SerializeField] private float volume = 1;
    [SerializeField] private bool playOnce = true;

    private void Start()
    {
        interactable = GetComponent<Interactable>();

        switch (triggerType)
        {
            case TriggerType.OnHighlight:
                interactable.highlightEvent += PlayClip;
                break;
            case TriggerType.OnTrigger:
                interactable.triggerEvent += PlayClip;
                break;
            case TriggerType.OnUntrigger:
                interactable.untriggerEvent += PlayClip;
                break;
            case TriggerType.OnUnHighlight:
                interactable.unhiglightEvent += PlayClip;
                break;
        }
    }

    private void OnDestroy()
    {
        switch (triggerType)
        {
            case TriggerType.OnHighlight:
                interactable.highlightEvent -= PlayClip;
                break;
            case TriggerType.OnTrigger:
                interactable.triggerEvent -= PlayClip;
                break;
        }
    }

    private void PlayClip(Movement movement)
    {
        SoundSystem.Play(sound, this.transform, SoundPriority.None, false, volume);

        if (playOnce)
            OnDestroy();
    }

}