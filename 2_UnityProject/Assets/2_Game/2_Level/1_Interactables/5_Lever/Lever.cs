using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : Interactable
{
    [SerializeField]float activationTimeInSeconds = 2;

    bool interactable = true;

    [SerializeField]Interactable activatable;

    void Awake()
    {
        AddTriggerAction(()=>ActivateSwitch());
    }

    void ActivateSwitch()
    {
        //Debug.Log ("Test");
        if (activatable)
        {
            interactable = false;
            StartCoroutine(ActivationCoroutine());
        }
            
    }

    IEnumerator ActivationCoroutine()
    {
        Debug.Log ("Triggered");
        activatable.Trigger();
        yield return new WaitForSeconds(activationTimeInSeconds);
        Debug.Log ("Untriggered");
        activatable.Untrigger();
        interactable = true;
    }

}
