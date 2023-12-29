using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ActivateOnAnyKey : MonoBehaviour
{
    [Serializable]
    private class ButtonUnityEvent : UnityEvent { }

    [SerializeField]
    private ButtonUnityEvent buttonEvent;

    private void Start()
    {
        CustomInputs inputMapping = CustomEventSystem.GetInputMapping;
        inputMapping.InUI.AnyKey.performed += InvokeEvent;
    }

    private void InvokeEvent(InputAction.CallbackContext context)
    {
        buttonEvent.Invoke();

        CustomInputs inputMapping = CustomEventSystem.GetInputMapping;
        inputMapping.InUI.AnyKey.performed -= InvokeEvent;

        Destroy(this);
    }
}
