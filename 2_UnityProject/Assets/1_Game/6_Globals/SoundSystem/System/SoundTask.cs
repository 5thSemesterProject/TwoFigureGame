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
    public FadeMode fadeMode = FadeMode.None;
    public float fadeDuration = 0;
    public float delay = 0;
    public float maxDistance = 5;
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
    private IEnumerator _FadeOut(float duration, float delayAfter = 0)
    {
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            audioSource.volume = Mathf.Lerp(volume, 0, elapsedTime / duration);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        if (delayAfter < 0)
            yield return new WaitForSecondsRealtime(Mathf.Abs(delay));

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
        FadeMode resumefadeMode = (fadeMode == FadeMode.FadeOut || fadeMode == FadeMode.FadeInOut) ? FadeMode.FadeOut : FadeMode.None;
        coroutine = StartCoroutine(_PlaySound(this ,audioSource, Mathf.Clamp(delay,-1, 0), fadeDuration, resumefadeMode));
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
        audioSource.dopplerLevel = 0;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = 0.2f;
        audioSource.maxDistance = maxDistance;
        audioSource.loop = loop;

        audioSource.Play();

        StopCoroutine();
        if (loop)
            return;
        coroutine = StartCoroutine(_PlaySound(this, audioSource, delay, fadeDuration, fadeMode));
    }

    private IEnumerator _PlaySound(SoundTask soundTask,AudioSource source, float delay, float crossFade, FadeMode fadeMode)
    {
        if (delay > 0)
        {
            source.Pause();
            yield return new WaitForSecondsRealtime(delay);
            source.UnPause();
        }

        switch (fadeMode)
        {
            case FadeMode.FadeInOut:
            case FadeMode.FadeIn:
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
                break;
            case FadeMode.None:
            case FadeMode.FadeOut:
            case FadeMode.Default:
            default:
                break;
        }

        float addedCrossfadeTime = (fadeMode == FadeMode.FadeInOut || fadeMode == FadeMode.FadeOut) ? crossFade : 0;
        float timeToWait = source.clip.length - source.time - addedCrossfadeTime;

        yield return new WaitForSecondsRealtime(timeToWait);

        switch (fadeMode)
        {
            case FadeMode.FadeInOut:
            case FadeMode.FadeOut:
                yield return _FadeOut(crossFade, Mathf.Abs(Mathf.Clamp(delay, -1, 0)));
                break;
            case FadeMode.None:
            case FadeMode.FadeIn:
            case FadeMode.Default:
                if (delay < 0)
                    yield return new WaitForSecondsRealtime(Mathf.Abs(delay));

                Remove();
                break;
        }
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
