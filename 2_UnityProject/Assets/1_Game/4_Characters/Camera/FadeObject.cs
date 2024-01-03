using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeObject : MonoBehaviour
{
    //Serialized for debuging
    [Header("Materials")]
    [SerializeField] private Material originalMaterial;
    [SerializeField] private Material instanceMaterial;

    [Header("Settings")]
    [SerializeField] private float minAlpha = 0.5f;
    [SerializeField] private float maxAlpha = 1f;
    public float fadeDuration = 1;
    public bool shouldMatchSharedMaterial = true;
    private bool isFadingIn = false;

    //References
    private Coroutine faderoutine;

    #region Enable / Disable
    private void OnEnable()
    {
        if (minAlpha > maxAlpha)
            maxAlpha = minAlpha;

        var notYetRemovedFadeObjectScripts = GetComponents<FadeObject>();
        foreach (var script in notYetRemovedFadeObjectScripts)
            if (script != this)
                Destroy(script);

        originalMaterial = GetComponent<Renderer>().sharedMaterial;
        instanceMaterial = GetComponent<Renderer>().material;

        HideObject(true);
    }
    private void OnDisable()
    {
        GetComponent<Renderer>().material = originalMaterial;
    }
    #endregion

    #region Show / Hide Object
    public void HideObject(bool force = false)
    {
        if (!force && !isFadingIn)
            return;

        isFadingIn = false;

        if (faderoutine != null)
            StopCoroutine(faderoutine);

        faderoutine = StartCoroutine(_FadeObject(false));
    }
    public void ShowObject()
    {
        if (isFadingIn)
            return;

        isFadingIn = true;

        if (faderoutine != null)
            StopCoroutine(faderoutine);

        faderoutine = StartCoroutine(_FadeObject(true));
    }
    #endregion

    #region Match Shared Material
    private void Update()
    {
        if (shouldMatchSharedMaterial)
            MatchSharedMaterial();
    }
    private void MatchSharedMaterial()
    {
        instanceMaterial.SetFloat("_Radius", originalMaterial.GetFloat("_Radius"));
        instanceMaterial.SetVector("_Epicenter", originalMaterial.GetVector("_Epicenter"));
        instanceMaterial.SetInt("_ShouldFade", originalMaterial.GetInt("_ShouldFade"));
    }
    #endregion

    #region Fade Logic
    private IEnumerator _FadeObject(bool fadeIn)
    {
        float elapsedTime = 0;
        float currentAlpha = instanceMaterial.GetFloat("_Alpha");
        float targetAlpha = fadeIn ? maxAlpha : minAlpha;
        float alphaLerpAmount = Mathf.Clamp01(maxAlpha - minAlpha);
        if (alphaLerpAmount == 0)
        {
            Debug.LogError($"Max Alpha is equal to Min Alpha! {gameObject.name}");
            yield break;
        }
        float currentProgress = Mathf.Clamp01((currentAlpha - minAlpha) / alphaLerpAmount);
        elapsedTime = fadeIn ? currentProgress : 1 - currentProgress;
        instanceMaterial.SetFloat("_Alpha", minAlpha + alphaLerpAmount * currentProgress);

        while (elapsedTime < 1)
        {
            float alpha = Mathf.Lerp(currentAlpha, targetAlpha, elapsedTime);
            instanceMaterial.SetFloat("_Alpha", alpha);

            //Debug.Log(alpha);

            elapsedTime += Time.deltaTime / fadeDuration;
            yield return null;
        }

        instanceMaterial.SetFloat("_Alpha", targetAlpha);

        faderoutine = null;

        if (fadeIn)
            Destroy(this);
    }
    #endregion
}
