using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.HighDefinition;

public enum ActivationMode
{
    Player, Box
}

[RequireComponent(typeof(PassOnTrigger),typeof(Interactable)) ]
public class PressurePlate : MonoBehaviour
{
    [SerializeField] GameObject button;
    [SerializeField] float pressAmount = 0.05f;
    [SerializeField] float pressSpeed = 0.1f;
    [SerializeField] ActivationMode activationMode;
    [SerializeField] float boxPositionTolerance = 0.1f;

    Vector3 initialButtonPos;
    Vector3 targetPos;
    Coroutine coroutine;
    Interactable interactable;
    NavMeshObstacle navMeshObstacle;

    bool pressed;

    void Awake()
    {   
        interactable = GetComponent<Interactable>();

        //Add Actions
        interactable.triggerEvent+=TriggerAction;
        interactable.untriggerEvent+=UntriggerAction;


        //Set Conditions
        if (activationMode ==ActivationMode.Player)
        {
            TriggerOnEnter triggerOnEnter;
            
            if (!TryGetComponent(out triggerOnEnter))
                triggerOnEnter = gameObject.AddComponent<TriggerOnEnter>();

            triggerOnEnter.AddTriggerCond(CheckPressed);
            triggerOnEnter.AddUntriggerCond(UntriggerCondition);
        }
        else if (activationMode==ActivationMode.Box)
        {
            if (TryGetComponent(out TriggerOnEnter triggerOnEnter))
                 Destroy(triggerOnEnter);
        }


        if (button!=null)
        {
            targetPos = button.transform.position;
            initialButtonPos = button.transform.position;
            coroutine = StartCoroutine(PressDownAnim());
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out MoveBox moveBox) && activationMode == ActivationMode.Box)
        {
            Vector2 moveBoxPos = VectorHelper.Convert3To2(moveBox.transform.position);
            Vector2 pressurePlatePos = VectorHelper.Convert3To2(moveBox.transform.position);

            if (Vector2.Distance(moveBoxPos,pressurePlatePos)<boxPositionTolerance)
                interactable.Trigger(moveBox.currentMover);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out MoveBox moveBox)&& activationMode == ActivationMode.Box)
        {
            interactable.Untrigger(moveBox.currentMover);
        }
    }

    void OnValidate()
    {
        if (button == null)
            Debug.LogWarning("Missing buton. Add a button to the Pressure Plate on "+ gameObject.name);
    }

    bool CheckPressed (Movement movement)
    {
        return !pressed;
    }

    void TriggerAction(Movement movement)
    {
        movement.interactable = interactable;
        targetPos = initialButtonPos+Vector3.down*pressAmount;
        pressed = true;
    }

    bool UntriggerCondition(Movement movement)
    {
        return pressed;
    }

    void UntriggerAction(Movement movement)
    {   
        //Remove Trigger from NavMesh
        if (navMeshObstacle!=null)
            navMeshObstacle.enabled = true;

        movement.interactable = null;
        targetPos = initialButtonPos;
        pressed = false;
    }

    IEnumerator PressDownAnim()
    {
        targetPos = button.transform.position;
        float t = 0;

        while (true)
        {
            Vector3 curentPos = Vector3.Lerp(button.transform.position,targetPos,t);
            button.transform.position = curentPos;
            t+=Time.deltaTime*pressSpeed;
            yield return null;
        }
    }

}


