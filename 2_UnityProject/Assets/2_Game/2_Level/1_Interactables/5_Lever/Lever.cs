using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Reflection;
using System;

[RequireComponent(typeof(Interactable),typeof(PassOnTrigger),typeof(TriggerByCharacter))]
public class Lever : MonoBehaviour
{
    [SerializeField]float activationTimeInSeconds = 2;

    Interactable interactable;
    bool usable = true;

    void Start()
    {
        interactable = GetComponent<Interactable>();
        interactable.triggerEvent+=ActivateSwitch;
        
        //Wait for switch to reactivate
        GetComponent<PassOnTrigger>().AddTriggerCond(CheckUsable);
    }

    bool CheckUsable(Movement movement)
    {
        return usable;
    }

    void ActivateSwitch(Movement movement)
    {
        usable = false;
        StartCoroutine(ActivationCoroutine(movement));     
    }

    IEnumerator ActivationCoroutine(Movement movement)
    {
        yield return new WaitForSeconds(activationTimeInSeconds);
        interactable.Untrigger(movement);
        usable = true;
    }
}

