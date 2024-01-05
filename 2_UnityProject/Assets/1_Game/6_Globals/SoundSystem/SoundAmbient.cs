using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundAmbient : MonoBehaviour
{
    [SerializeField] private EAmbientSounds sound;
    [SerializeField] private float volume = 1;
    [SerializeField] private bool loop = true;

    private IEnumerator Start()
    {
        while (true)
        {
            if (SoundSystem.Play(sound, this.transform, SoundPriority.None, loop, volume))
                yield break;

            yield return null;
        }
    }
}
