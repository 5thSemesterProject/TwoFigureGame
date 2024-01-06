using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundBackground : MonoBehaviour
{
    [SerializeField] private EMusic backgroundMusic;
    [SerializeField] private float volume = 1;
    [SerializeField] private bool loop = true;
    [SerializeField] private FadeMode fadeMode = FadeMode.None;
    [SerializeField] private float fadeDuration = 0f;
    [SerializeField] private bool playOnEnable = true;
    private Coroutine coroutine;

    private void OnEnable()
    {
        if (playOnEnable)
            TriggerSound();
    }

    public void TriggerSound()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(_TriggerSound());
    }

    private IEnumerator _TriggerSound()
    {
        while (true)
        {
            if (SoundSystem.Play(backgroundMusic, this.transform, SoundPriority.None, loop, volume, 0, fadeMode, fadeDuration))
            {
                coroutine = null;
                yield break;
            }

            yield return null;
        }
    }
}
