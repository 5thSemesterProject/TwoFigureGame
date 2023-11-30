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
        Debug.Log ("ActivateSwitch");
        usable = false;
        StartCoroutine(ActivationCoroutine(movement));     
    }

    IEnumerator ActivationCoroutine(Movement movement)
    {
        yield return new WaitForSeconds(activationTimeInSeconds);
        interactable.Untrigger(movement);
        usable = true;
    }

    void  OnDestroy()
    {
        Debug.Log ("test");
    }


    //can be defined anywhere doesn't need to be in the same Class or as an extension method on MonoBehaviour
    public void DestroyRequiredComponents(MonoBehaviour monoInstanceCaller)
    {
        MemberInfo memberInfo = monoInstanceCaller.GetType();
        RequireComponent[] requiredComponentsAtts = Attribute.GetCustomAttributes(memberInfo, typeof(RequireComponent), true) as RequireComponent[];
        foreach (RequireComponent rc in requiredComponentsAtts)
        {
            if (rc != null && monoInstanceCaller.GetComponent(rc.m_Type0) != null)
            {
                Destroy(monoInstanceCaller.GetComponent(rc.m_Type0));
            }
        }
    }
}

