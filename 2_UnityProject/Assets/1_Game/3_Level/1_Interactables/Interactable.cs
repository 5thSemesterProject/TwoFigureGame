using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.TextCore.Text;

public enum CharacterType
{
    None, Woman, Man
}

public enum TriggerType
{
    OnHighlight, OnTrigger, OnUntrigger, OnUnHighlight
}

public class Interactable : MonoBehaviour
{
    public CharacterType specificCharacterAccess;

    public delegate void ActionDel(Movement movement);

    public delegate bool Condition(Movement movement);

    public event ActionDel enterEvent;
     public event ActionDel exitEvent;
     public event ActionDel triggerEvent;
    public event ActionDel untriggerEvent;
    public event ActionDel highlightEvent;
    public event ActionDel unhiglightEvent;
    public event ActionDel aiEnterEvent;
    public event ActionDel aiExitEvent;
    public event ActionDel aiStayEvent;

    public Condition exitCond;
    public Condition enterCond;
    public Condition highlightCond; 
    public Condition unhighlightCond;

    void Awake()
    {
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

        Debug.LogWarning("No Trigger Collider added. Make sure there is a Trigger Collider on "+ gameObject.name);
    }

    void OnTriggerEnter(Collider other)
    {   
        CharacterData activeCharacterData = CharacterManager.ActiveCharacterData;

        Movement  movementComp=null;
        Transform parent = other.transform.parent;
        if (parent!=null)
            movementComp = parent.GetComponent<Movement>();

        if (movementComp!=null)
        {   
            //Handle Active Character    
            if (activeCharacterData.currentState.GetType() != typeof(AIState) &&activeCharacterData.movement == movementComp)
            {
                if (specificCharacterAccess == CharacterType.None || specificCharacterAccess == movementComp.characterType)
                {

                    if (enterCond!=null && enterCond(movementComp)||enterCond==null)
                        enterEvent?.Invoke(movementComp);

                    if (highlightCond!=null && highlightCond(movementComp)||highlightCond==null)
                        highlightEvent?.Invoke(movementComp);
                }
            }

            //Handle inactive AI Character
            else if (activeCharacterData.currentState.GetType() != typeof(AIState) &&activeCharacterData.movement != movementComp)
            {
                aiEnterEvent?.Invoke(movementComp);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        Movement  movementComp=null;
        Transform parent = other.transform.parent;
        if (parent!=null)
            movementComp = parent.GetComponent<Movement>();
        
        if (movementComp!=null)
        {
            CharacterData activeCharacterData = CharacterManager.ActiveCharacterData;
            if (activeCharacterData.movement == movementComp)
            {
                if (exitCond!=null&&exitCond(movementComp)||exitCond==null)
                   exitEvent?.Invoke(movementComp);

                if (unhighlightCond!=null && unhighlightCond(movementComp)||unhighlightCond==null)
                    unhiglightEvent?.Invoke(movementComp);
            }

            //Handle inactive AI Character
            else if (activeCharacterData.movement != movementComp)
                aiExitEvent?.Invoke(movementComp);
               
        }
    }

    void OnTriggerStay(Collider other)
    {
        CharacterData activeCharacterData = CharacterManager.ActiveCharacterData;

        Movement  movementComp=null;
        Transform parent = other.transform.parent;
        if (parent!=null)
            movementComp = parent.GetComponent<Movement>();

        if (movementComp!=null)
        {
            //Handle Active Player
            if (activeCharacterData.currentState.GetType() != typeof(AIState) &&activeCharacterData.movement == movementComp)
            {
                if (specificCharacterAccess == CharacterType.None || specificCharacterAccess == movementComp.characterType && activeCharacterData.movement == movementComp)
                {
                    if (enterCond!=null &&enterCond(movementComp)||enterCond==null)
                        enterEvent?.Invoke(movementComp);
    
                    if (highlightCond!=null && highlightCond(movementComp)||highlightCond==null)
                        highlightEvent?.Invoke(movementComp);
                }
            }

            //Handle inactive AI Character
            else if (activeCharacterData.movement != movementComp)
            {
                aiStayEvent?.Invoke(movementComp);
            }
                
        }
    }

    public void Trigger(Movement movement)
    {
        if (enterCond!=null &&enterCond(movement)||enterCond==null)
            triggerEvent?.Invoke(movement);
    }

    public void Untrigger(Movement movement)
    {      
        if (exitCond!=null &&exitCond(movement)||exitCond==null)
            untriggerEvent?.Invoke(movement);
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



