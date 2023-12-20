using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class TriggerOnEnter : MonoBehaviour
{
    Interactable interactable;
    Interactable.Condition additionalTriggerCond;
    Interactable.Condition additionalUntriggerCond;

    void  Start()
    {
        interactable = GetComponent<Interactable>();
        interactable.enterEvent += TriggerAction;
        interactable.exitEvent+=UntriggerAction;
    }

    public void AddUntriggerCond(Interactable.Condition condition)
    {
        additionalUntriggerCond = condition;
    }   

   public void AddTriggerCond(Interactable.Condition condition)
    {
        additionalTriggerCond=condition;
    }

    void TriggerAction(Movement movement)
    {
        if (additionalTriggerCond !=null && additionalTriggerCond(movement)||additionalTriggerCond==null)
        {
            interactable.Trigger(movement);
        }
            
    }

    void UntriggerAction(Movement movement)
    {
        if (additionalUntriggerCond !=null && additionalUntriggerCond(movement)||additionalUntriggerCond==null)
            interactable.Untrigger(movement);
    }
    
}
