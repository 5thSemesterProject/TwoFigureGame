using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundAmbient : MonoBehaviour
{
    [SerializeField] private EAmbientSounds sound;
    [SerializeField] private float volume = 1;
    [SerializeField] private bool loop = true;
    [SerializeField] private float maxRange = 5;
    [Range(0.5f, 1.5f)][SerializeField] private float minPitch = 0.8f;
    [Range(0.5f, 1.5f)][SerializeField] private float maxPitch = 1.2f;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxRange);
    }

    private IEnumerator Start()
    {
        while (true)
        {
            if (SoundSystem.Play(sound, this.transform, SoundPriority.None, loop, volume, 0, FadeMode.Default, 0, maxRange))
                yield break;

            yield return null;
        }
    }
}
