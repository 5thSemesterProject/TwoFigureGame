using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    Interactable prevTriggeredBy;
    [SerializeField] public Interactable triggeredBy;
    [SerializeField] Interactable triggering;
    [SerializeField] bool showLinesWhenNotSelected;
    Interactable prevTriggering;

    /// <summary>
    /// Called when another object exits the collider of this interactable.
    /// </summary>
    /// <param name="other">The collider of the other object.</param>
    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Movement>(out Movement movementComp))
            movementComp.interactable = this;
    }

    /// <summary>
    /// Triggers the interaction behavior of this interactable.
    /// </summary>
    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Movement>(out Movement movementComp))
            movementComp.interactable = null;
    }

    /// <summary>
    /// The main functionality of this interactable.
    /// </summary>
    public void Trigger()
    {
        if (triggering==null)
            Function();
        else
            triggering.Trigger();
    }

    
    protected virtual void Function(){}

    /// <summary>
    /// Highlights the interactable object.
    /// </summary>
    protected virtual void Highlight(){}

#region Running in Editor

    void OnValidate()
    {
        // Ensure consistency when triggeredBy is changed
        if (triggeredBy != prevTriggeredBy)
        {
            CheckForLoop(triggeredBy, this);

            if (prevTriggeredBy != null)
            {
                prevTriggeredBy.triggering = null;
            }

            if (triggeredBy != null)
            {
                triggeredBy.triggering = this;
            }

            prevTriggeredBy = triggeredBy;
        }

        // Ensure consistency when triggering is changed
        if (triggering != prevTriggering)
        {
            CheckForLoop(triggering, this);

            if (prevTriggering != null)
            {
                prevTriggering.triggeredBy = null;
            }

            if (triggering != null)
            {
                triggering.triggeredBy = this;
            }

            prevTriggering = triggering;
        }

    }
      void CheckForLoop(Interactable target, Interactable source)
{
    if (target != null && source != null)
    {
        Interactable current = target;

        // Check for a loop by traversing the triggering chain
        while (current != null)
        {
            if (current == source)
            {
                Debug.LogWarning("Warning: Creating a loop in triggering relationships. This can lead to unexpected behavior.");
                break;
            }

            current = current.triggering;
        }
    }
}

    void DrawLines()
    {
         if (triggeredBy != null)
        {
            Handles.color = Color.red;
            Handles.DrawAAPolyLine(20f,triggeredBy.transform.position, transform.position);
            DrawArrow(triggeredBy.transform.position, transform.position);
        }

        if (triggering != null)
        {
            Handles.color = Color.green;
            Handles.DrawAAPolyLine(20f,triggering.transform.position, transform.position);
            DrawArrow(triggering.transform.position, transform.position);
        }
    }

    void DrawArrow(Vector3 start,Vector3 end)
    {
        Vector3 direction = end-start;
        direction = direction.normalized;

        Vector3 leftEndPoint = end + Quaternion.Euler(new Vector3(0,-225,0))*direction*0.2f;
        Vector3 rightEndPoint = end + Quaternion.Euler(new Vector3(0,225,0))*direction*0.2f;

        Handles.color = Color.blue;
        Handles.DrawAAPolyLine(20f,end, leftEndPoint);
        Handles.DrawAAPolyLine(20f,end, rightEndPoint);
    }

   void OnDrawGizmosSelected()
    {
       DrawLines();
    }

    void  OnDrawGizmos()
    {
        if (showLinesWhenNotSelected)
            DrawLines();
    }  
    #endregion
}


