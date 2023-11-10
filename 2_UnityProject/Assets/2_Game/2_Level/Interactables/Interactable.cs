using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] Interactable triggeredBy;
    [SerializeField] Interactable triggering;

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Movement>(out Movement movementComp))
            movementComp.interactable = this;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Movement>(out Movement movementComp))
            movementComp.interactable = null;
    }

    public void Trigger()
    {
        if (triggering!=null)
            Function();
        else
            triggering.Trigger();
    }

    protected virtual void Function(){}

}
