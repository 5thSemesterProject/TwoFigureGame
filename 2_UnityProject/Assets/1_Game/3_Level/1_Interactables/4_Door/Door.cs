using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Interactable))]
public class Door : MonoBehaviour

{
    Animator animator;
    Interactable interactable;

    OffMeshLink offMeshLink;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        interactable = GetComponent<Interactable>();

        interactable.triggerEvent+=OpenDoor;
        interactable.untriggerEvent+=CloseDoor;

        offMeshLink = interactable.GetComponentInChildren<OffMeshLink>();
        if (offMeshLink!=null)
            offMeshLink.activated = false;
    }
    public void OpenDoor(Movement movement)
    {
        animator.SetBool("Open",true);

        if (offMeshLink!=null)
            offMeshLink.activated = true;
    }

    public void CloseDoor(Movement movement)
    {
        animator.SetBool("Open",false);

        if (offMeshLink!=null)
            offMeshLink.activated = false;
    }


}
