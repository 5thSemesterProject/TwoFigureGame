using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[RequireComponent(typeof(Interactable))]
public class ChangeLightOnHighlight : MonoBehaviour
{
    [SerializeField] float highlightIntensity;
    [SerializeField] Light mainLight;
    [SerializeField] Light[] bounceLights;
    [SerializeField] float switchDuration = 0.2f;
    
    HighlightLight mainHighlight;
    HighlightLight []bounceHighlights;


    Interactable interactable;
    bool highlighted = false;


    void Start()
    {
        if (mainLight!=null)
        {
            //Subscribe Events         
            interactable = GetComponent<Interactable>();
            interactable.highlightEvent+= Highlight;
            interactable.highlightCond = CheckHighlighted;
            interactable.unhiglightEvent+=Unhighlight;

            //Setup MainHighlight
            mainHighlight = mainLight.AddComponent<HighlightLight>();
            mainHighlight.SetHighlightIntensity(highlightIntensity);
            float highlightRise = mainHighlight.GetHighlightRiseInPercent();


            //Setup BounceLights
            bounceHighlights = new HighlightLight[bounceLights.Length];
            for (int i = 0; i < bounceLights.Length; i++)
            {
                if (bounceLights!=null)
                {
                    bounceHighlights[i] = bounceLights[i].AddComponent<HighlightLight>();
                    bounceHighlights[i].SetHighlightIntensityInPercent(Mathf.Sqrt(highlightRise));
                }    
                else
                    Debug.LogWarning("Bounce Light is null!");
            }

        }
        else
            Debug.LogWarning("Missing light on "+gameObject.name);


        
    }

     void Highlight(Movement movement)
    {
        highlighted = true;
        
        mainHighlight.Highlight();
        for (int i = 0; i < bounceHighlights.Length; i++)
            bounceHighlights[i].Highlight();

    }

    void Unhighlight(Movement movement)
    {
        highlighted = false;

        mainHighlight.Unhighlight();
        
        for (int i = 0; i < bounceHighlights.Length; i++)
            bounceHighlights[i].Unhighlight();

    }

    bool CheckHighlighted(Movement movement)
    {
        return !highlighted;
    }

   
}
