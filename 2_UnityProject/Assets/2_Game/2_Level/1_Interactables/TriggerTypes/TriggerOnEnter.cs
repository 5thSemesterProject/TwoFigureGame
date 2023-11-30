using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class TriggerOnEnter : MonoBehaviour
{
    Interactable interactable;
    Interactable.Condition additionalTriggerCond;

    void  Start()
    {
        interactable = GetComponent<Interactable>();
        interactable.enterEvent += TriggerAction;
    }

   public void AddTriggerCond(Interactable.Condition condition)
    {
        additionalTriggerCond=condition;
    }

    void TriggerAction(Movement movement)
    {
        if (additionalTriggerCond !=null && additionalTriggerCond(movement))
            interactable.Trigger(movement);
    }
}
