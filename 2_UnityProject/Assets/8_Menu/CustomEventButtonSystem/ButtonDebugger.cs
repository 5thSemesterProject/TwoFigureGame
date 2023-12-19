using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class ButtonDebugger : CustomButtonFunctionality
{
    [SerializeField] private string defaultButton;
    [SerializeField] private string backButton;
    [SerializeField] private string activeButton;

    protected override void OnAwake()
    {
        AddFunctionToEvent(() => Debug.Log("Button Debugger: Hover"), ButtonEventType.Hover);
        AddFunctionToEvent(() => Debug.Log("Button Debugger: Click"), ButtonEventType.Click);
        AddFunctionToEvent(() => Debug.Log("Button Debugger: NoHover"), ButtonEventType.NoHover);
    }

    private void Update()
    {
        if (CustomEventSystem.DefaultButton != null)
        {
            defaultButton = CustomEventSystem.DefaultButton.name;
        }
        if (CustomEventSystem.current.activeButton != null)
        {
            activeButton = CustomEventSystem.current.activeButton.name;
        }
        if (CustomEventSystem.BackButton != null)
        {
            backButton = CustomEventSystem.BackButton.name;
        }
    }
}
