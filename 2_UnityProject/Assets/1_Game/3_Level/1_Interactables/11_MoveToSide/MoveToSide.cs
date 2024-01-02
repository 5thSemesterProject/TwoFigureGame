using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Interactable),typeof(Animator))]
public class MoveToSide : MonoBehaviour
{
    Animator animator;
    Interactable interactable;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        interactable = GetComponent<Interactable>();

        interactable.triggerEvent+=SlideOpen;
        interactable.untriggerEvent+=SlideClose;
    }
    public void SlideOpen(Movement movement)
    {
        animator.SetBool("Open",true);
    }

    public void SlideClose(Movement movement)
    {
        animator.SetBool("Open",false);
    }
}

