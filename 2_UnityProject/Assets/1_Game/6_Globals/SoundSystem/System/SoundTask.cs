using System.Collections;
using UnityEngine;

public class SoundTask : MonoBehaviour
{

    private AudioSource audioSource;
    private Coroutine coroutine;
    private bool fadingOut = false;

    //Attributes
    public AudioClip clip;
    public float volume = 1;
    public float crossfadeDuration = 0;
    public float delay = 0;
    public bool spatialize = false;
    public bool loop = false;
    public uint priority = 1;
    [HideInInspector] public SoundChannel channel = null;
    public bool IsFadingOut { get => fadingOut; }

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    #region Stop
    public void Remove()
    {
        if (gameObject.tag == "AudioObject")
            Destroy(gameObject);
        else
        {
            Destroy(audioSource);
            if (channel != null)
                channel.Remove(this);
            Destroy(this);
        }
    }
    public void FadeOut(float duration)
    {
        if (fadingOut)
            return;
        fadingOut = true;
        StopCoroutine(coroutine);
        coroutine = StartCoroutine(_FadeOut(duration));
    }
    private IEnumerator _FadeOut(float duration)
    {
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            audioSource.volume = Mathf.Lerp(volume, 0, elapsedTime / duration);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        Remove();
    }
    #endregion

    #region Pause
    public void Pause()
    {
        if (fadingOut)
            return;
        audioSource.Pause();
        StopCoroutine();
    }
    public void Resume()
    {
        if (fadingOut)
            return;
        StopCoroutine();
        audioSource.UnPause();
        if (loop)
            return;
        coroutine = StartCoroutine(_PlaySound(audioSource, 0, 0));
    }
    #endregion

    #region Play
    public void Play()
    {
        if (clip == null)
        {
            Debug.LogWarning("No clip when played!");
            Remove();
        }
        fadingOut = false;
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.spatialBlend = spatialize ? 1 : 0;
        audioSource.loop = loop;

        audioSource.Play();

        StopCoroutine();
        if (loop)
            return;
        coroutine = StartCoroutine(_PlaySound(audioSource, delay, crossfadeDuration));
    }

    private IEnumerator _PlaySound(AudioSource source, float delay, float crossFade)
    {
        if (delay > 0)
        {
            source.Pause();
            yield return new WaitForSecondsRealtime(delay);
            source.UnPause();
        }

        if (crossFade > 0)
        {
            float elapsedTime = 0;
            float targetVolume = source.volume;
            source.volume = 0;
            while (elapsedTime < crossFade)
            {
                source.volume = Mathf.Lerp(0, targetVolume, elapsedTime / crossFade);
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            source.volume = targetVolume;
        }

        yield return new WaitForSecondsRealtime(source.clip.length - source.time);

        if (delay < 0)
            yield return new WaitForSecondsRealtime(Mathf.Abs(delay));

        Remove();
    }
    #endregion

    #region Utility
    private void StopCoroutine()
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
    }
    #endregion
}
