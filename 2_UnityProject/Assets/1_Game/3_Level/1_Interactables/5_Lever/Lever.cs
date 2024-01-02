using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

[RequireComponent(typeof(Interactable),typeof(PassOnTrigger),typeof(TriggerByCharacter))]
public class Lever : MonoBehaviour
{
    [SerializeField]float activationTimeInSeconds = 2;

    [Header ("Handle")]
    [SerializeField] GameObject handle;
    [SerializeField] float rotateSpeed = 0.1f;
    [SerializeField] float rotationAngle = 120;

    Quaternion initialHandleRot;
    Quaternion targetRot;

    

    Interactable interactable;
    bool usable = true;

    void Start()
    {
        interactable = GetComponent<Interactable>();
        interactable.triggerEvent+=ActivateSwitch;
        
        //Wait for switch to reactivate
        interactable.enterCond = CheckUsable;

        initialHandleRot = handle.transform.localRotation;
        StartCoroutine(RotateHandle());
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
        targetRot = Quaternion.Euler(new Vector3(-rotationAngle,0,0));
        yield return new WaitForSeconds(activationTimeInSeconds);
        usable = true;
        interactable.Untrigger(movement);
        targetRot = initialHandleRot;
    }

    IEnumerator RotateHandle()
    {
        targetRot = handle.transform.localRotation;
        float t = 0;

        while (true)
        {
            Quaternion currenRot = Quaternion.Lerp(handle.transform.localRotation,targetRot,t);
            handle.transform.localRotation = currenRot;
            t+=Time.deltaTime*rotateSpeed*0.01f;
            yield return null;
        }
    }
}

