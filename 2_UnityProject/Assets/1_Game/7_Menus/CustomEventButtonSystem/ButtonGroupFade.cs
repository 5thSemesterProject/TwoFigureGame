using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ButtonEnabler))]
public class ButtonGroupFade : MonoBehaviour
{
    public float fadeSpeed = 0.3f;
    public bool hiddenOnEnable = true;
    public bool fadeOnEnable = true;
    private ButtonEnabler buttonEnabler;
    private Coroutine fadeRoutine;

    private void OnEnable()
    {
        buttonEnabler = GetComponent<ButtonEnabler>();
        if (hiddenOnEnable)
        {
            buttonEnabler.canvasGroup.alpha = 0;
        }
        if (fadeOnEnable)
        {
            FadeIn();
        }
    }

    public void FadeIn()
    {
        ReplaceCoroutine(Fade(1, fadeSpeed));
    }
    public void Remove()
    {
        FadeOut(true);
    }
    public float FadeOut(bool destroy = true)
    {
        if (destroy)
            ReplaceCoroutine(FadeAndDestroy(0, fadeSpeed));
        else
            ReplaceCoroutine(Fade(0, fadeSpeed));
        return fadeSpeed;
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
            float tempValue = Mathf.Lerp(currentValue, targetValue, timeElapsed);
            buttonEnabler.canvasGroup.alpha = tempValue;
            CustomLogic(timeElapsed, tempValue);

            timeElapsed += Time.unscaledDeltaTime / time;
            yield return null;
        }

        buttonEnabler.canvasGroup.alpha = targetValue;
        CustomLogic(1, targetValue);

        fadeRoutine = null;
    }

    //To be overriden
    protected virtual void CustomLogic(float timeElapsed, float currentValue)
    {

    }
}
