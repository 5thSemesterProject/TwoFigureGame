using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class Door : Interactable
{
    Animator animator;
    Interactable interactable;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        interactable = GetComponent<Interactable>();

        interactable.triggerEvent+=OpenDoor;
        interactable.untriggerEvent+=CloseDoor;
    }
    public void OpenDoor(Movement movement)
    {
        animator.SetBool("Open",true);
    }

    public void CloseDoor(Movement movement)
    {
        animator.SetBool("Open",false);
    }


}
