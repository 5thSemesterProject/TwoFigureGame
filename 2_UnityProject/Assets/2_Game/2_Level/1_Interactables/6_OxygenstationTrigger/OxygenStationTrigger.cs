using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(Oxygenstation))]
public class OxygenStationTrigger : Interactable
{
    Oxygenstation oxygenstation;

    void  Awake()
    {
        oxygenstation = GetComponent<Oxygenstation>();

    }

    void Update()
    {
        CheckActivateTriggering();
    }

    void CheckActivateTriggering()
    {
        if (oxygenstation?.GetAmountOfCharacters()>1)
        {
            SetTriggering(true);
            Destroy (this);
        }
            
    }


}
