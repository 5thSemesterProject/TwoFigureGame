using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Interactable),typeof(PassOnTrigger))]
public class WaitForTriggers : MonoBehaviour
{

    [SerializeField] PassOnTrigger[] triggersToConsider;

    int amountOfTriggers;
    int loggedInTrigggers;
    Interactable interactable;

    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<Interactable>();
        interactable.triggerEvent +=Trigger;
        interactable.untriggerEvent +=Untrigger;
        
        amountOfTriggers = triggersToConsider.Length;
        
        for (int i = 0; i < triggersToConsider.Length; i++)
        {
            triggersToConsider[i].OverwriteTriggering(interactable);
        }

        GetComponent<PassOnTrigger>().AddTriggerCond(CompareTriggers);
    }

    void  OnValidate()
    {
        for (int i = 0; i < triggersToConsider.Length; i++)
        {
            if (triggersToConsider[i]!=null &&triggersToConsider[i].GetTriggering()!=null)
                Debug.LogError("Pls dont map anything to the triggering item with the name" + triggersToConsider[i].name);
        }
    }

    void Trigger(Movement movement)
    {
        loggedInTrigggers++;

    }

    void Untrigger(Movement movement)
    {
        loggedInTrigggers--;
    }

    bool CompareTriggers(Movement movement)
    {
        return loggedInTrigggers>=amountOfTriggers;
    }
}
