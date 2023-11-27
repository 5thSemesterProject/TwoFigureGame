using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : Interactable
{
    [SerializeField]float activationTimeInSeconds = 2;

    bool interactable = true;

    void Awake()
    {
        AddTriggerAction(()=>ActivateSwitch());
    }

    void ActivateSwitch()
    {
        //Debug.Log ("Test");
        if (interactable)
        {
            interactable = false;
            StartCoroutine(ActivationCoroutine());
        }
            
    }

    IEnumerator ActivationCoroutine()
    {
        Debug.Log ("Triggered");
        yield return new WaitForSeconds(activationTimeInSeconds);
        Debug.Log ("Untriggered");
        Untrigger();
        interactable = true;
    }

}
