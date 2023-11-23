using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomFadeLogic : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Material[] materials;
    [SerializeField] private int playerLayer = 9;
    [SerializeField] private float maxFadeRadius = 50;
    [SerializeField] private float fadeTime = 1;

    private Coroutine fadeRoutine;

    //Shader Variable Names
    private string nameRadius = "_Radius";
    private string nameShouldFade = "_ShouldFade";
    private string nameEpicenter = "_Epicenter";

    void Start()
    {
        if (materials == null || materials.Length == 0)
        {
            Debug.LogWarning("Something went wrong. No material assigned.");
        }
        else
        {
            SetVisible(false);
        }
    }

    private void SetVisible(bool visible)
    {
        SetMaterialInt(nameShouldFade, visible ? 0 : 1);
        SetMaterialFloat(nameRadius, visible ? maxFadeRadius : 0);
    }

    /*
    private Material[] GetAllMaterialsInChildren(string targetShaderName)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        List<Material> materials = new List<Material>();
        List<string> materialsAdded = new List<string>();

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.shader.name == targetShaderName)
            {
                if (!materialsAdded.Contains(renderers[i].sharedMaterial.name))
                {
                    materialsAdded.Add(renderers[i].sharedMaterial.name);
                    materials.Add(renderers[i].sharedMaterials[0]);
                }
            }
        }

        return materials.ToArray();
    }
    */

    #region Triggers
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == playerLayer)
        {
            SetMaterialVector(nameEpicenter, VectorHelper.Convert3To2(other.transform.position));
            StartFade(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == playerLayer)
        {
            SetMaterialVector(nameEpicenter, VectorHelper.Convert3To2(other.transform.position));
            StartFade(false);
        }
    }
    #endregion

    #region Fade Logic
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
        //Get - Set Targets
        float currentRadius = materials[0].GetFloat(nameRadius);
        float targetRadius = bFadeIn ? maxFadeRadius : 0;
        SetMaterialFloat(nameRadius, currentRadius);

        //Make Fadable
        SetMaterialInt(nameShouldFade, 1);

        //Set Radius Over Time
        while (bFadeIn ? currentRadius < targetRadius : targetRadius < currentRadius)
        {
            float addedDeltaTime = bFadeIn ? Time.deltaTime : -Time.deltaTime;
            currentRadius += addedDeltaTime * maxFadeRadius / fadeTime;
            SetMaterialFloat(nameRadius, currentRadius);
            yield return null;
        }

        //Turn Off Fade if is visible
        SetVisible(bFadeIn);

        fadeRoutine = null;
    }
    #endregion

    #region Set Material Values
    private void SetMaterialInt(string intName, int value)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetInt(intName, value);
        }
    }
    private void SetMaterialFloat(string floatName, float value)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetFloat(floatName, value);
        }
    }
    private void SetMaterialVector(string vectorName, Vector4 value)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetVector(vectorName, value);
        }
    }
    #endregion
}
