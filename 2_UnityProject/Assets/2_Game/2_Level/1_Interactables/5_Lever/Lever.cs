using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Interactable),typeof(PassOnTrigger),typeof(TriggerByCharacter))]
public class Lever : Interactable
{
    [SerializeField]float activationTimeInSeconds = 2;

    Interactable interactable;
    bool usable = true;

    void Start()
    {
        interactable = GetComponent<Interactable>();
        interactable.triggerEvent+=ActivateSwitch;
        
        //Wait for switch to reactivate
        GetComponent<PassOnTrigger>().AddTriggerCond(CheckActive);
    }

    bool CheckActive(Movement movement)
    {
        return usable;
    }

    void ActivateSwitch(Movement movement)
    {
        if (usable)
        {
            usable = false;
            StartCoroutine(ActivationCoroutine(movement));
        }
            
    }

    IEnumerator ActivationCoroutine(Movement movement)
    {
        yield return new WaitForSeconds(activationTimeInSeconds);
        interactable.Untrigger(movement);
        usable = true;
    }

}
