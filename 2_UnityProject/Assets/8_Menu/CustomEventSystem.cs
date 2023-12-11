using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public enum NavigateDirections
{
    Up,
    Down,
    Left,
    Right,
}

[Tooltip("Trash but better than Unitys")]
[RequireComponent(typeof(EventSystem))]
public class CustomEventSystem : MonoBehaviour
{
    //Internal
    public static CustomEventSystem current;
    public bool startWithDefaultHovered = true;

    //Important Buttons
    public static CustomButton selectedButton;
    public static CustomButton hoveredButton;
    private CustomButton defaultButton;
    private CustomButton currentSelection
    {
        get 
        {
            if (hoveredButton == null)
            {
                if (selectedButton == null)
                {
                    return defaultButton;
                }
                return selectedButton;
            }
            return hoveredButton;
        }
    }

    //Custom Input
    public static CustomInputs GetInputMapping { get => current.inputMapping; }
    private CustomInputs inputMapping;

    //Controller Dampening
    [Header("Controller Delay")]
    [SerializeField] private float firstClickDelay = 0.5f;
    [SerializeField] private float minimumClickDelay = 0.1f;
    private NavigateDirections controllerDirection;
    private Coroutine navigationRoutine;

    #region Singleton
    private void Awake()
    {
        if (current == null)
            current = this;
        else
            Destroy(this);

        DontDestroyOnLoad(this);

        //Set up custom inputs and default button
        inputMapping = new CustomInputs(); //Custom
        defaultButton = GetDefaultButton();

        //Register Navigation Callbacks
        SubscribeCallbacks();
    }

    private void Start()
    {
        //Hover Default button if enabled
        if (startWithDefaultHovered && defaultButton != null)
        {
            defaultButton.HoverLogic();
        }
    }

    private static CustomButton GetDefaultButton()
    {
        CustomButton[] customButtons = GameObject.FindObjectsOfType<CustomButton>();

        if (customButtons == null || customButtons.Length <= 0 || customButtons[0] == null)
        {
            Debug.LogWarning("No Buttons Existent");
            return null;
        }

        for (int i = 0; i < customButtons.Length; i++)
        {
            if (customButtons[i].isDefaultButton)
            {
                return customButtons[i];
            }
        }

        return customButtons[0];
    }

    private void SubscribeCallbacks() //Custom
    {
        inputMapping.InUI.Submit.performed += OnSubmit;
        inputMapping.InUI.Navigate.performed += TriggerNavigation;
        inputMapping.InUI.Navigate.canceled += TriggerNavigation;
    }

    private void OnDisable()
    {
        if (current == this)
        {
            current = null;
        }

        inputMapping.InUI.Submit.performed -= OnSubmit;
        inputMapping.InUI.Navigate.performed -= TriggerNavigation;
        inputMapping.InUI.Navigate.canceled -= TriggerNavigation;
    }
    #endregion

    #region Control Scheme Switching
    public static void SwitchControlScheme(InputActionMap actionMap)
    {
        var customInputMaps = GetInputMapping.asset.actionMaps;
        for (int i = 0; i < customInputMaps.Count; i++)
        {
            if (customInputMaps[i].name == actionMap.name)
            {
                DisableControlSchemes();
                actionMap.Enable();
                Debug.Log("Enabled" + actionMap.name);
            }
        }
    }

    public static void DisableControlSchemes()
    {
        foreach (var actionMap in GetInputMapping.asset.actionMaps)
        {
            if (actionMap.enabled)
            {
                actionMap.Disable();
                Debug.Log("Disabled" + actionMap.name);
            }
        }
    }
    #endregion

    public static void UpdateSelectedButton(CustomButton newSelect)
    {
        if (newSelect == selectedButton)
            return;

        if (newSelect.state != ButtonState.Selected)
        {
            Debug.LogWarning("Selected button was not in the selected state!");
        }

        if (selectedButton != null)
        {
            if (selectedButton.holdsPointer == false)
            {
                selectedButton.state = ButtonState.None;
                selectedButton.NoHoverLogic();
            }
            else
            {
                selectedButton.state = ButtonState.Hovered;
                selectedButton.HoverLogic();
            }
        }

        selectedButton = newSelect;

        if (hoveredButton != null && selectedButton == hoveredButton)
        {
            hoveredButton = null;
        }
    }

    public void OnSubmit(InputAction.CallbackContext context)
    {
        if (hoveredButton == null)
            return;

        hoveredButton.ClickLogic();
    }

    #region Navigation
    private void TriggerNavigation(InputAction.CallbackContext context)
    {
        //If Canceled
        if (context.phase == InputActionPhase.Canceled)
        {
            if (navigationRoutine != null)
            {
                StopCoroutine(navigationRoutine);
            }
            navigationRoutine = null;
            return;
        }

        //Read Direction
        Vector2 navigateVector = context.ReadValue<Vector2>();
        NavigateDirections direction = GetNavigateDirection(navigateVector);

        //Return if direction has not changed
        if (navigationRoutine != null && direction == controllerDirection)
            return;

        controllerDirection = direction;

        //Stop Coroutine if running
        if (navigationRoutine != null)
        {
            StopCoroutine(navigationRoutine);
        }

        //Start Succession
        navigationRoutine = StartCoroutine(SuccessionNavigation(controllerDirection));
    }

    private IEnumerator SuccessionNavigation(NavigateDirections direction)
    {
        Navigate(direction);

        yield return new WaitForSeconds(firstClickDelay);

        while (true)
        {
            Navigate(direction);

            yield return new WaitForSeconds(minimumClickDelay);
        }
    }

    public void Navigate(NavigateDirections direction)
    {
        CustomButton nextButton = null;
        switch (direction)
        {
            case NavigateDirections.Up:
                nextButton = currentSelection.navigation.up;
                break;
            case NavigateDirections.Down:
                nextButton = currentSelection.navigation.down;
                break;
            case NavigateDirections.Left:
                nextButton = currentSelection.navigation.left;
                break;
            case NavigateDirections.Right:
                nextButton = currentSelection.navigation.right;
                break;
            default:
                break;
        }

        if (nextButton == null)
            return;

        nextButton.HoverLogic();
    }

    private NavigateDirections GetNavigateDirection(Vector2 navigateVector)
    {
        if (Mathf.Abs(navigateVector.x) > Mathf.Abs(navigateVector.y))
        {
            if (navigateVector.x < 0)
            {
                return NavigateDirections.Left;
            }
            return NavigateDirections.Right;
        }
        if (navigateVector.y < 0)
        {
            return NavigateDirections.Down;
        }
        return NavigateDirections.Up;
    }
    #endregion
}
