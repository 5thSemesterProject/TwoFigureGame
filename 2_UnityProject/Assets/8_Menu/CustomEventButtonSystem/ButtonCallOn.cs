using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonCallOn : CustomButtonFunctionality
{
    [SerializeField]
    private ButtonEventType activeTriggerMoment = ButtonEventType.Click;

    [Serializable]
    private class ButtonUnityEvent : UnityEvent { }

    [SerializeField]
    private ButtonUnityEvent buttonEvent;

    private void Start()
    {
        AddFunctionToEvent(buttonEvent.Invoke ,activeTriggerMoment);
    }
}
