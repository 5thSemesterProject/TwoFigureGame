using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;

[RequireComponent(typeof(CustomPassVolume))]
public class Blur : MonoBehaviour
{
    [SerializeField] private Material blurterial;
    private CustomPassVolume customPassVolume;

    private void OnEnable()
    {
        customPassVolume = GetComponent<CustomPassVolume>();
        customPassVolume.targetCamera = Camera.main;
        SetBlur(0);
    }

    private void OnDisable()
    {
        SetBlur(4);
    }

    public void SetBlur(float value)
    {
        blurterial.SetFloat("_Blur", value);
    }
}
