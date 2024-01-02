using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ActivateOnAnyKey : MonoBehaviour
{
    [Serializable]
    private class ButtonUnityEvent : UnityEvent { }

    [SerializeField]
    private ButtonUnityEvent buttonEvent;

    [SerializeField]
    private bool returnToInGame = false;

    private void Start()
    {
        CustomInputs inputMapping = CustomEventSystem.GetInputMapping;
        CustomEventSystem.SwitchControlScheme(inputMapping.InUI);
        inputMapping.InUI.AnyKey.performed += InvokeEvent;
    }

    private void InvokeEvent(InputAction.CallbackContext context)
    {
        buttonEvent.Invoke();

        CustomInputs inputMapping = CustomEventSystem.GetInputMapping;
        inputMapping.InUI.AnyKey.performed -= InvokeEvent;

        if (returnToInGame)
        {
            CustomEventSystem.SwitchControlScheme(inputMapping.InGame);
        }

        Destroy(this);
    }
}
