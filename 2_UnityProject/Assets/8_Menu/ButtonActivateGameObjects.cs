using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonActivateGameObjects : CustomButtonFunctionality
{
    [SerializeField]
    private GameObject objectToActivate;

    [SerializeField]
    private ButtonEventType activeTriggerMoment = ButtonEventType.Hover;
    [SerializeField]
    private ButtonEventType inActiveTriggerMoment = ButtonEventType.NoHover;

    private void Start()
    {
        if (objectToActivate == null)
            return;

        AddFunctionToEvent(() => objectToActivate.SetActive(true), activeTriggerMoment);

        if (activeTriggerMoment == inActiveTriggerMoment)
            return;

        AddFunctionToEvent(() => objectToActivate.SetActive(false), inActiveTriggerMoment);
    }
}
