using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (Interactable))]
public class PassOnTrigger : MonoBehaviour
{
    [SerializeField] Interactable triggering;
    Interactable interactable;

    Interactable.Condition additionalTriggerCond;
    

    void Start() 
    {
        
        interactable = GetComponent<Interactable>();
        interactable.triggerEvent += TriggerOtherInteractable; 
        interactable.untriggerEvent+= UntriggerOtherInteractable;

       
        if (triggering == interactable)
            Debug.LogError("Item can't trigger itself! Set a differen trigger on "+ gameObject.name);

    }

    public Interactable GetTriggering()
    {
        return triggering;
    }

    public void AddTriggerCond(Interactable.Condition condition)
    {
        additionalTriggerCond=condition;
    }

    void TriggerOtherInteractable(Movement movement)
    {
        if (additionalTriggerCond!=null && additionalTriggerCond(movement)||additionalTriggerCond==null)
            triggering?.Trigger(movement);
    }

    void UntriggerOtherInteractable(Movement movement)
    {
        triggering?.Untrigger(movement);
    }

    public void OverwriteTriggering(Interactable triggering)
    {
        if (triggering!=null)
            Debug.LogWarning("Overwriting trigger and Conditons on "+gameObject.name);

        this.triggering = triggering;
    }


}
