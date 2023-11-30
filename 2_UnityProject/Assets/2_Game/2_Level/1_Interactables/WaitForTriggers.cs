using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Interactable),typeof(PassOnTrigger))]
public class WaitForTriggers : MonoBehaviour
{

    [SerializeField]int requiredTriggers;
    int loggedInTrigggers;
    Interactable interactable;

    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<Interactable>();
        interactable.triggerEvent +=Trigger;
        interactable.untriggerEvent +=Untrigger;

        GetComponent<PassOnTrigger>().AddTriggerCond(CompareTriggers);
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
        return loggedInTrigggers>=requiredTriggers;
    }
}
