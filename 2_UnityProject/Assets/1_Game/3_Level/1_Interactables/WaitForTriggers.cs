using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Interactable)/*,typeof(PassOnTrigger)*/)]
public class WaitForTriggers : MonoBehaviour
{

    [SerializeField]int requiredTriggers;
    public int numberOfLoggedInTriggers { get { return loggedInTriggers.Count; }     private set { }}
    List <Movement> loggedInTriggers = new List<Movement>();
    Interactable interactable;

    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<Interactable>();
        interactable.untriggerEvent +=Untrigger;
        interactable.enterCond =CompareTriggers;
        interactable.exitEvent +=Untrigger;
    }

    void Untrigger(Movement movement)
    {
        if (loggedInTriggers.Contains(movement))
            loggedInTriggers.Remove(movement);
    }

    bool CompareTriggers(Movement movement)
    {
        if (!loggedInTriggers.Contains(movement))
            loggedInTriggers.Add(movement);
        return numberOfLoggedInTriggers>=requiredTriggers;
    }
}
