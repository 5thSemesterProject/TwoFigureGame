using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(Oxygenstation),typeof(Interactable))]
public class OxyStatInteractableBlocker : MonoBehaviour
{
    Oxygenstation oxygenstation;
    [SerializeField] Interactable interactableToBlock;

    Interactable.Condition blockedTriggerCond;
    Interactable.Condition blockedUntriggerCond;

    void  Awake()
    {
        oxygenstation = GetComponent<Oxygenstation>();
        
        //Block all Interaction
        blockedTriggerCond = interactableToBlock.triggerCond;
        interactableToBlock.triggerCond = BlockInteractable;
        blockedUntriggerCond = interactableToBlock.untriggerCond;
        interactableToBlock.untriggerCond = BlockInteractable;
    }

    bool BlockInteractable(Movement movement)
    {
        return false;
    }

    void Update()
    {
        CheckActivateTriggering();
    }

    void CheckActivateTriggering()
    {
        if (oxygenstation?.GetAmountOfCharacters()>1)
        {
            interactableToBlock.triggerCond = blockedTriggerCond;
            interactableToBlock.untriggerCond = blockedUntriggerCond;
            Destroy (this);
        }
            
    }


}
