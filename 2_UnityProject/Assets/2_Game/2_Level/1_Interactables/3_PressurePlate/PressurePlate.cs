using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public enum ActivationMode
{
    Player, Box
}

[RequireComponent(typeof(PassOnTrigger),typeof(TriggerOnEnter),typeof(Interactable)) ]
public class PressurePlate : MonoBehaviour
{
    [SerializeField] GameObject button;
    [SerializeField] float pressAmount = 0.05f;
    [SerializeField] float pressSpeed = 0.1f;
    [SerializeField] ActivationMode activationMode;

    Vector3 initialButtonPos;
    Vector3 targetPos;
    Coroutine coroutine;

    Interactable interactable;

    bool pressed;

    void Start()
    {   
        interactable = GetComponent<Interactable>();

        if (interactable==null)
            Debug.Log ("Test");

        //Add Actions
        interactable.triggerEvent+=TriggerAction;
        interactable.untriggerEvent+=UntriggerAction;

        //Set Conditions
        interactable.exitCond = UntriggerCondition;

        //Only Trigger when button is not pressed
        GetComponent<TriggerOnEnter>().AddTriggerCond(CheckPressed);

        if (button == null)
            Debug.LogError("Missing buton. Add a button to the Pressure Plate on "+ gameObject.name);


        targetPos = button.transform.position;
        initialButtonPos = button.transform.position;

        coroutine = StartCoroutine(PressDownAnim());
    }

    bool CheckPressed (Movement movement)
    {
        return !pressed;
    }

    void TriggerAction(Movement movement)
    {
        targetPos = initialButtonPos+Vector3.down*pressAmount;
        pressed = true;
    }

    bool UntriggerCondition(Movement movement)
    {
        return pressed;
    }

    void UntriggerAction(Movement movement)
    {
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


