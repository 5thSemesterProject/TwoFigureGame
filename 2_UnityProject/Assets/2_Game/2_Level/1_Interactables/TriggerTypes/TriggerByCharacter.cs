using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (Interactable))]
public class TriggerByCharacter : MonoBehaviour
{
    Interactable interactable;
    
    void  Start()
    {
        interactable = GetComponent<Interactable>();
        interactable.enterEvent+= AddInteractable;
        interactable.untriggerEvent+= RemoveInteractable;
    }

    void AddInteractable(Movement movement)
    {
        movement.interactable = interactable;
    }

    void RemoveInteractable (Movement movement)
    {
        movement.interactable = null;
    }
}

