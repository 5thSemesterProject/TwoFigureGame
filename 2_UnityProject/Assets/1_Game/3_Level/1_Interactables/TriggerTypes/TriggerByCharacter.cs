using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (Interactable))]
public class TriggerByCharacter : MonoBehaviour
{
    Interactable interactable;

    bool triggered;
    
    void  Start()
    {
        interactable = GetComponent<Interactable>();
        interactable.enterEvent+= AddInteractable;
        interactable.exitEvent+=RemoveInteractable;
    }

    void AddInteractable(Movement movement)
    {
        if (movement.interactable!=interactable)
            movement.interactable = interactable;
    }

    void RemoveInteractable (Movement movement)
    {
        movement.interactable = null;
    }

    public void Activate(Movement movement)
    {
        if (triggered)
            interactable.Untrigger(movement);
        else
            interactable.Trigger(movement);

        triggered = !triggered;
    }
}

