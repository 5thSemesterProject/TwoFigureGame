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
    bool open = false;
    float doorOpenTime = 2;

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
        StartCoroutine(_OpenDoor());
    }

    public void CloseDoor(Movement movement)
    {
        animator.SetBool("Open", false);
        open = false;
    }

    public bool GetOpen()
    {
        return open;
    }

   IEnumerator _OpenDoor()
   {
        yield return new WaitForSeconds(doorOpenTime);
        open = true;
   }
}
