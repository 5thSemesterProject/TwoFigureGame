using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Door : Interactable
{
    Animator animator;

    void  Start()
    {
        animator = GetComponent<Animator>();
        AddTriggerAction(()=>OpenDoor());
        AddUntriggerAction(()=>CloseDoor());
    }
    public void OpenDoor()
    {
        animator.SetBool("Open",true);
    }

    public void CloseDoor()
    {
        animator.SetBool("Open",false);
    }

    protected override void Highlight()
    {
        
    }


}
