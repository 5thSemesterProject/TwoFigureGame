using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ButtonEnabler))]
public class ButtonGroupFade : MonoBehaviour
{
    public float fadeSpeed = 0.3f;
    public bool fadeOnEnable = true;
    private ButtonEnabler buttonEnabler;
    private Coroutine fadeRoutine;

    private void OnEnable()
    {
        buttonEnabler = GetComponent<ButtonEnabler>();
        if (fadeOnEnable)
        {
            EnterFromZero();
        }
    }

    public void EnterFromZero()
    {
        buttonEnabler.canvasGroup.alpha = 0;
        FadeIn();
    }
    public float ExitAndDestroy()
    {
        ReplaceCoroutine(FadeAndDestroy(1, fadeSpeed));
        return fadeSpeed;
    }
    public void FadeIn()
    {
        ReplaceCoroutine(Fade(1, fadeSpeed));
    }
    public void FadeOut()
    {
        ReplaceCoroutine(Fade(0, fadeSpeed));
    }

    private void ReplaceCoroutine(IEnumerator coroutine)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(coroutine);
    }

    public IEnumerator FadeAndDestroy(float targetValue, float time = 1)
    {
        yield return Fade(targetValue, time);
        Destroy(gameObject);
    }
    public IEnumerator Fade(float targetValue, float time = 1)
    {
        float currentValue = buttonEnabler.canvasGroup.alpha;
        targetValue = Mathf.Clamp01(targetValue);

        float timeElapsed = 0;

        while (timeElapsed < 1)
        {
            buttonEnabler.canvasGroup.alpha = Mathf.Lerp(currentValue, targetValue, timeElapsed);
            timeElapsed += Time.unscaledDeltaTime / time;
            yield return null;
        }

        buttonEnabler.canvasGroup.alpha = targetValue;

        fadeRoutine = null;
    }
}
