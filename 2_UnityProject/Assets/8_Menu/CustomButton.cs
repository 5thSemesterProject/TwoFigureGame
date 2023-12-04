using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum ButtonState
{
    None,
    Hovered,
    Selected,
}

public enum ButtonEventType 
{
    None,
    Click,
    Hover,
    NoHover,
}

public delegate void ButtonEvent();

public class CustomButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, ISubmitHandler
{
    public event ButtonEvent clickEvent;
    public event ButtonEvent hoverEvent;
    public event ButtonEvent hoverEndEvent;

    [ReadOnly]
    public ButtonState state;

    #region Default Logic
    private void RaiseClick()
    {
        clickEvent?.Invoke();
    }
    private void RaiseHover()
    {
        hoverEvent?.Invoke();
    }
    private void RaiseHoverEnd()
    {
        hoverEndEvent?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            return;

        state = ButtonState.Selected;
        RaiseClick();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (state.Equals(ButtonState.Selected))
        {
            //Soft Hover can be added here
            return;
        }

        state = ButtonState.Hovered;
        RaiseHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (state.Equals(ButtonState.Selected))
            return;

        state = ButtonState.None;
        RaiseHoverEnd();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (state.Equals(ButtonState.Selected))
        {
            //Soft Hover can be added here
            return;
        }

        state = ButtonState.Hovered;
        RaiseHover();
    }
    #endregion
}

[RequireComponent(typeof(CustomButton))]
public class CustomButtonFunctionality : MonoBehaviour
{
    protected CustomButton button;

    private void Awake()
    {
        button = GetComponent<CustomButton>();
    }

    protected void AddFunctionToEvent(ButtonEvent action, ButtonEventType triggerMoment)
    {
        switch (triggerMoment)
        {
            case ButtonEventType.Click:
                button.clickEvent += action;
                break;
            case ButtonEventType.Hover:
                button.hoverEvent += action;
                break;
            case ButtonEventType.NoHover:
                button.hoverEndEvent += action;
                break;
            case ButtonEventType.None:
                break;
        }
    }
}