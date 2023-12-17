using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

#region Structs
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

[Serializable]
public struct NavigationButtons
{
    public CustomButton up;
    public CustomButton right;
    public CustomButton down;
    public CustomButton left;
    private CustomButton self;

    public NavigationButtons(CustomButton selfButton)
    {
        up = null;
        right = null;
        down = null;
        left = null;
        self = selfButton;
    }
}

public delegate void ButtonEvent();
#endregion

public class CustomButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    //Events
    public event ButtonEvent clickEvent;
    public event ButtonEvent hoverEvent;
    public event ButtonEvent hoverEndEvent;

    //Public Attributes
    [ReadOnly]
    public ButtonState state;
    public bool holdsPointer = false;
    public bool isDefaultButton = false;
    public bool isBackButton = false;
    public bool interactable = true;

    //Navigation
    [Header("Navigation")]
    public NavigationButtons navigation = new NavigationButtons();

    #region Enable/Disable
    private void OnEnable()
    {
        if (isBackButton)
            CustomEventSystem.BackButton = this;

        if (isDefaultButton)
            CustomEventSystem.DefaultButton = this;
    }

    private void OnDisable()
    {
        if (isBackButton && CustomEventSystem.BackButton == this)
            CustomEventSystem.BackButton = null;

        if (isDefaultButton && CustomEventSystem.DefaultButton == this)
            CustomEventSystem.DefaultButton = null;
    }
    #endregion

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
        //Return if rightclicked
        if (eventData.button == PointerEventData.InputButton.Right)
            return;

        ClickLogic();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        holdsPointer = true;

        HoverLogic();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        holdsPointer = false;

        if (CustomEventSystem.hoveredButton == this && CustomEventSystem.selectedButton == null)
            return;

        NoHoverLogic();
    }

    public void ClickLogic()
    {
        //Return if button is not interactable or the eventsystem is disabled
        if (!interactable || !CustomEventSystem.InputEnabled)
            return;

        state = ButtonState.Selected;
        CustomEventSystem.UpdateSelectedButton(this);
        RaiseClick();
    }
    public void HoverLogic()
    {
        //Return if button is not interactable or the eventsystem is disabled
        if (!interactable || !CustomEventSystem.InputEnabled)
            return;

        if (CustomEventSystem.hoveredButton != null)
        {
            CustomEventSystem.hoveredButton.NoHoverLogic();
            CustomEventSystem.hoveredButton = null;
        }

        if (state.Equals(ButtonState.Selected))
        {
            //Soft Hover can be added here
            return;
        }

        CustomEventSystem.hoveredButton = this;

        state = ButtonState.Hovered;
        RaiseHover();
    }
    public void NoHoverLogic()
    {
        //Return if button is not interactable or the eventsystem is disabled
        if (!interactable || !CustomEventSystem.InputEnabled)
            return;

        if (state == ButtonState.Selected)
            return;

        if (CustomEventSystem.hoveredButton == this)
            CustomEventSystem.hoveredButton = null;

        state = ButtonState.None;
        RaiseHoverEnd();
    }
    #endregion
}

[RequireComponent(typeof(CustomButton))]
public class CustomButtonFunctionality : MonoBehaviour
{
    protected CustomButton button;

    #region Awake
    private void Awake()
    {
        button = GetComponent<CustomButton>();
        OnAwake();
    }

    //To be overridden by children to access the awake method
    protected virtual void OnAwake()
    {

    }
    #endregion

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