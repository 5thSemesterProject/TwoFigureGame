using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class ChangeLightOnHighlight : MonoBehaviour
{
    Color normal;
    [SerializeField] Color highlight;
    [SerializeField] Light lightInput;
    [SerializeField] float colorSwitchDuration = 0.2f;
    Color targetColor;

    Interactable interactable;
    Coroutine coroutine;
    bool highlighted = false;


    void Start()
    {
        interactable = GetComponent<Interactable>();
        
        if (lightInput!=null)
        {
            //Setup for light color changing
            targetColor = lightInput.color;
            normal = targetColor;
            
            interactable.highlightEvent+= Highlight;
            interactable.highlightCond = CheckHighlighted;
            interactable.unhiglightEvent+=Unhighlight;
        }
        else
            Debug.LogWarning("Missing light on "+gameObject.name);
    }

    bool CheckHighlighted(Movement movement)
    {
        return !highlighted;
    }

    void Highlight(Movement movement)
    {
        highlighted = true;
        targetColor = highlight;

        if (coroutine==null)
            coroutine = StartCoroutine(_LerpColor());
    }

    void Unhighlight(Movement movement)
    {
        highlighted = false;
        targetColor = normal;

        if (coroutine==null)
            coroutine = StartCoroutine(_LerpColor());
    }


    IEnumerator _LerpColor()
    {
        Color currentColor = GetColor();
        float smoothTime = colorSwitchDuration;
        float velocity = 0;

        while (true)
        {
            currentColor.r = Mathf.SmoothDamp(currentColor.r, targetColor.r, ref velocity, smoothTime);
            currentColor.g = Mathf.SmoothDamp(currentColor.g, targetColor.g, ref velocity, smoothTime);
            currentColor.b = Mathf.SmoothDamp(currentColor.b, targetColor.b, ref velocity, smoothTime);

            if (Mathf.Abs(currentColor.r - targetColor.r) < 0.01f
                &&Mathf.Abs(currentColor.g - targetColor.g) < 0.01f
                &&Mathf.Abs(currentColor.b - targetColor.b) < 0.01f)
            {
                currentColor = targetColor;
                SetColor(targetColor);
                break;
            }

            SetColor(currentColor);

            yield return null;
        }

        coroutine = null;
        
    }

    Color GetColor()
    {
        return lightInput.color;
    }

    void SetColor(Color color)
    {
        lightInput.color = color;
    }
}
