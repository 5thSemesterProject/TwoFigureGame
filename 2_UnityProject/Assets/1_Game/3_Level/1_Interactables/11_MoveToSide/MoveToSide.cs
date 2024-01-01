using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Interactable),typeof(Animator))]
public class MoveToSide : MonoBehaviour
{
    Animator animator;
    Interactable interactable;

    private int activePlatesCount = 0;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        interactable = GetComponent<Interactable>();

        interactable.triggerEvent += PlateActivated;
        interactable.untriggerEvent += PlateDeactivated;
    }

    public void PlateActivated(Movement movement)
    {
        activePlatesCount++;
        animator.SetBool("Open", true);
    }

    public void PlateDeactivated(Movement movement)
    {
        activePlatesCount--;

        if (activePlatesCount <= 0)
        {
            activePlatesCount = 0;
            animator.SetBool("Open", false);
        }
    }
}

