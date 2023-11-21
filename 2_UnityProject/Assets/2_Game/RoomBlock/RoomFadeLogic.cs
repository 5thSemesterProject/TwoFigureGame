using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomFadeLogic : MonoBehaviour
{
    [SerializeField] private Material material;
    [SerializeField] private int playerLayer = 9;
    [SerializeField] private float maxFadeRadius = 50;
    [SerializeField] private float fadeTime = 1;

    private Coroutine fadeRoutine;

    void Start()
    {
        if (material == null)
        {
            material = GetComponentInChildren<Renderer>().material;
            if (material == null)
            {
                Debug.LogWarning("Something went wrong. No material found.");
            }
        }

        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        material.SetInt("_ShouldFade", visible ? 0 : 1);
        material.SetFloat("_Radius", visible ? maxFadeRadius : 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered");

        if (other.gameObject.layer == playerLayer)
        {
            Debug.Log("CorrectLayer");
            material.SetVector("_Epicenter", VectorHelper.Convert3To2(other.transform.position));
            StartFade(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == playerLayer)
        {
            material.SetVector("_Epicenter", VectorHelper.Convert3To2(other.transform.position));
            StartFade(false);
        }
    }

    private void StartFade(bool bFadeIn)
    {
        //Stop Prior Fade
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        //Start Fade
        fadeRoutine = StartCoroutine(Fade(bFadeIn));
    }

    private IEnumerator Fade(bool bFadeIn)
    {
        //Get/Set Targets
        float currentRadius = material.GetFloat("_Radius");
        float targetRadius = bFadeIn ? maxFadeRadius : 0;

        //Make Fadable
        material.SetInt("_ShouldFade", 1);

        //Set Radius Over Time
        while (bFadeIn ? currentRadius < targetRadius : targetRadius < currentRadius)
        {
            float addedDeltaTime = bFadeIn ? Time.deltaTime : -Time.deltaTime;
            currentRadius += addedDeltaTime * maxFadeRadius / fadeTime;
            material.SetFloat("_Radius", currentRadius);
            yield return null;
        }

        //Turn Off Fade if is visible
        material.SetInt("_ShouldFade", bFadeIn ? 0 : 1);

        fadeRoutine = null;
    }
}
