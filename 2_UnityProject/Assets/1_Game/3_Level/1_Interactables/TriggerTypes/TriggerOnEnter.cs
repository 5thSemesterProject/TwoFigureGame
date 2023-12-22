using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Interactable))]
public class TriggerOnEnter : MonoBehaviour
{
    [SerializeField] private bool includeAICharacterStay = true;
    Interactable interactable;
    Interactable.Condition additionalTriggerCond;
    Interactable.Condition additionalUntriggerCond;

    NavMeshObstacle navMeshObstacle;

    void  Start()
    {
        interactable = GetComponent<Interactable>();
        interactable.enterEvent += TriggerAction;
        interactable.exitEvent+=UntriggerAction;

        if (includeAICharacterStay)
        {
            interactable.aiStayEvent +=TriggerAction;
        }

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
