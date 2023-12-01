using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Interactable)/*,typeof(PassOnTrigger)*/)]
public class WaitForTriggers : MonoBehaviour
{

    [SerializeField]int requiredTriggers;
    int loggedInTrigggers;
    Interactable interactable;

    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<Interactable>();
        interactable.untriggerEvent +=Untrigger;

        interactable.triggerCond =CompareTriggers;

        interactable.exitEvent +=Untrigger;
    }

    void Untrigger(Movement movement)
    {
        loggedInTrigggers--;
    }

    bool CompareTriggers(Movement movement)
    {
        loggedInTrigggers++;
        return loggedInTrigggers>=requiredTriggers;
    }
}
