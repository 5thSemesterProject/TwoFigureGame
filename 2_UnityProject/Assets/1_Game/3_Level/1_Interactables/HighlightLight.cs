using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class HighlightLight : MonoBehaviour
{

    Light lightComp;
    float switchDuration = 0.2f;
    
    float targetIntensity;
    float normalIntensity;
    float highlightIntensity;
    Coroutine intensityCoroutine;


    void  Awake()
    {
        lightComp = GetComponent<Light>();
        normalIntensity = lightComp.intensity;
    }

    public void SetHighlightIntensity(float intensity)
    {
        highlightIntensity = intensity;
    }

    public float GetHighlightRiseInPercent()
    {
        float difference = highlightIntensity-normalIntensity;
        float differenceInPercent = difference/normalIntensity;
        return differenceInPercent;
    }

    public void SetHighlightIntensityInPercent(float percentInput)
    {
        SetHighlightIntensity(percentInput*normalIntensity);
    }

    public void Highlight()
    {
        targetIntensity = highlightIntensity;

        if (intensityCoroutine==null)
            intensityCoroutine = StartCoroutine(_LerpIntensity());
    }

    public void Unhighlight()
    {
        targetIntensity = normalIntensity;

        if (intensityCoroutine==null)
            intensityCoroutine = StartCoroutine(_LerpIntensity());
    }

    IEnumerator _LerpIntensity()
    {
        float currentIntensity = GetIntensity();
        float smoothTime = switchDuration;
        float velocity = 0;

        while (true)
        {
            currentIntensity = Mathf.SmoothDamp(currentIntensity, targetIntensity, ref velocity, smoothTime);

            if (Mathf.Abs(currentIntensity-targetIntensity)<0.01f)
            {
                currentIntensity = targetIntensity;
                SetIntensity(targetIntensity);
                break;
            }
            
            SetIntensity(currentIntensity);

            yield return null;
        }

        intensityCoroutine = null;
    }


    float GetIntensity()
    {
        return lightComp.intensity;
    }

    void SetIntensity(float intensity)
    {
        lightComp.intensity = intensity;
    }
}
