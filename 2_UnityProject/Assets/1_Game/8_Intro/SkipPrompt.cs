using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent (typeof(CanvasGroup))]
public class SkipPrompt : MonoBehaviour
{
    [SerializeField] float skipPromptAppearance = 1;
    [SerializeField] Coroutine skipPromptProcess;
    [SerializeField] public UnityEvent onSkip;
    CustomInputs customInputMaps;
    CanvasGroup canvasGroup;
    Coroutine alphaCoroutine;
    float targetAlpha;


    void  Awake()
    {
        customInputMaps = CustomEventSystem.GetInputMapping;
        customInputMaps.InUI.AnyKey.performed+=ShowSkipPrompt;
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
    }

    void ShowSkipPrompt(InputAction.CallbackContext callbackContext)
    {
        if (skipPromptProcess!=null)
            StopCoroutine(skipPromptProcess);
        skipPromptProcess = StartCoroutine(SkipProcess());
    }

    void InvokeSkipEvent (InputAction.CallbackContext callbackContext)
    {
        customInputMaps.InUI.AnyKey.performed-=InvokeSkipEvent;
        customInputMaps.InUI.AnyKey.performed-=ShowSkipPrompt;
        onSkip?.Invoke();
    }

    IEnumerator SkipProcess()
    {
        customInputMaps.InUI.AnyKey.performed+=InvokeSkipEvent;
        customInputMaps.InUI.AnyKey.performed-=ShowSkipPrompt;
        LerpAlpha(1,0.1f);
        yield return new WaitForSeconds(skipPromptAppearance);
        customInputMaps.InUI.AnyKey.performed-=InvokeSkipEvent;
        customInputMaps.InUI.AnyKey.performed+=ShowSkipPrompt;
        LerpAlpha(0,0.1f);
        yield return null;
    }

    #region AlphaLerp
    public void LerpAlpha(float alpha, float smoothTime = 0.33f)
    {
        SetTargetAlpha(alpha);

        if (alphaCoroutine == null)
        {
            alphaCoroutine = StartCoroutine(_LerpAlpha(smoothTime));
        }
    }

    void SetTargetAlpha(float alpha) 
    {
        targetAlpha = alpha;
    }
    IEnumerator _LerpAlpha(float smoothTime=0.33f)
    {
        float currentAlpha = GetAlpha();
        float velocity = 0;

        while (true)
        {
            currentAlpha =  Mathf.SmoothDamp(currentAlpha, targetAlpha, ref velocity, smoothTime);

          if (Mathf.Abs(targetAlpha-currentAlpha)<0.01f)
          {
                currentAlpha = targetAlpha;
                SetAlpha(currentAlpha);
                break;
          }

            SetAlpha(currentAlpha);

            yield return null;
        }

        alphaCoroutine = null; 
    }

    void SetAlpha(float inputAlpha)
    {

        canvasGroup.alpha = inputAlpha;
    }

    float GetAlpha()
    {
        return canvasGroup.alpha;
    }
    #endregion 
}
