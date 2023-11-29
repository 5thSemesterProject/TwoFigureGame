using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;

public enum CharacterType
{
    None, Woman, Man
}

[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour
{
    [SerializeField]bool active = true;
    public CharacterType specificCharacterAccess;

    public delegate void ActionDel(Movement movement);

    public delegate bool Condition(Movement movement);

    public event ActionDel enterEvent;
     public event ActionDel untriggerEvent;
     public event ActionDel triggerEvent;

    public Condition exitCond;
    public Condition enterCond;

    void Awake()
    {
        enterCond = DefaultEnterCond;
        exitCond = DefaultExitCond;

        AssureColliders();
    }

    void AssureColliders()
    {
        var colliders = GetComponents<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].isTrigger)
                return;
        }

        Debug.LogWarning("No Trigger Collider added. Make sure there is a Trigger Collider. Setting one automatically to "+gameObject.name);
        colliders[0].isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Movement movementComp))
        {
            if (specificCharacterAccess == CharacterType.None || specificCharacterAccess == movementComp.characterType)
            {

                if (enterCond(movementComp) && active)
                    enterEvent?.Invoke(movementComp);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Movement movementComp))
        {
                if (exitCond(movementComp))
                   untriggerEvent?.Invoke(movementComp);
        }
    }

    void  OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out Movement movementComp))
        {
            if (specificCharacterAccess == CharacterType.None || specificCharacterAccess == movementComp.characterType)
            {
                if (enterCond(movementComp))
                    enterEvent?.Invoke(movementComp);
            }
        }
    }

    public void Trigger(Movement movement)
    {
        triggerEvent?.Invoke(movement);
    }

    public void Untrigger(Movement movement)
    {        
        untriggerEvent?.Invoke(movement);
    }

    public void SetTriggering(bool active)
    {
        this.active = active;
    }

    public bool DefaultEnterCond(Movement movement)
    {
        return true;
    }

    public bool DefaultExitCond(Movement movement)
    {
        return true;
    }

    /// <summary>
    /// Highlights the interactable object.
    /// </summary>

/*#region Running in Editor

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
            if (current == source && source!=null)
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
#if UNITY_EDITOR
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
#endif
    }
#if UNITY_EDITOR
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
#endif

    void OnDrawGizmosSelected()
    {
       DrawLines();
    }
    #endregion*/
}



