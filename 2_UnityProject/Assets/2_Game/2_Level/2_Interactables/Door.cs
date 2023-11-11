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
    }
    protected override void Function()
    {
        animator.SetBool("Open",true);
    }
    protected override void Highlight()
    {

    }

}
