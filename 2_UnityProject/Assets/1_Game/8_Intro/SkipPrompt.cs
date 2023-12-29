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
    [SerializeField] public UnityEvent skip;
    CustomInputs customInputMaps;
    CanvasGroup canvasGroup;
    Coroutine alphaCoroutine;
    float targetAlpha;


    void  Awake()
    {
        customInputMaps = CustomEventSystem.GetInputMapping;
        customInputMaps.InUI.AnyKey.performed+=ShowSkipPrompt;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void ShowSkipPrompt(InputAction.CallbackContext callbackContext)
    {
        customInputMaps.InUI.AnyKey.performed-=ShowSkipPrompt;
        customInputMaps.InUI.AnyKey.performed+=InvokeSkipEvent;
        if (skipPromptProcess!=null)
            StopCoroutine(skipPromptProcess);
        skipPromptProcess = StartCoroutine(SkipIntroProceess());
    }

    void InvokeSkipEvent (InputAction.CallbackContext callbackContext)
    {
        skip?.Invoke();
    }

    #region AlphaLerp
    IEnumerator SkipIntroProceess()
    {
        yield return new WaitForSeconds(skipPromptAppearance);
        //skipPrompt.SetActive(false);
        customInputMaps.InUI.AnyKey.performed-=InvokeSkipEvent;
        customInputMaps.InUI.AnyKey.performed+=ShowSkipPrompt;
        yield return null;
    }

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
