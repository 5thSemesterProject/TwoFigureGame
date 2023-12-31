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
    public static event ButtonEvent allNoHover;
    public bool startWithDefaultHovered = true;
    public bool recieveUIInput = true;
    public bool debug = false;
    public static bool InputEnabled { get => current.recieveUIInput; }

    //Important Buttons
    public static CustomButton selectedButton;
    public static CustomButton hoveredButton;
    private CustomButton backButton;
    private CustomButton defaultButton;
    public CustomButton activeButton
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
    public static CustomButton BackButton { get => current != null ? current.backButton : null; set => current.backButton = value; }
    public static CustomButton DefaultButton { get => current != null ? current.defaultButton : null; set => current.defaultButton = value; }

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

        //Set up custom inputs
        inputMapping = new CustomInputs(); //Custom

        //Register Navigation Callbacks
        SubscribeCallbacks();
    }

    private void Start()
    {
        //Reset Selection and Hover Default if enabled
        ResetSelectedButtons();
    }

    private static CustomButton GetDefaultButton()
    {
        CustomButton[] customButtons = GameObject.FindObjectsOfType<CustomButton>();

        if (customButtons == null || customButtons.Length <= 0 || customButtons[0] == null)
        {
            LogWarning("No Buttons Existent");
            return null;
        }

        for (int i = 0; i < customButtons.Length; i++)
        {
            if (customButtons[i].IsDefault && customButtons[i].IsInteractable)
            {
                return customButtons[i];
            }
        }

        return customButtons[0];
    }
    private static CustomButton GetBackButton()
    {
        CustomButton[] customButtons = GameObject.FindObjectsOfType<CustomButton>();

        if (customButtons == null || customButtons.Length <= 0 || customButtons[0] == null)
        {
            LogWarning("No Buttons Existent");
            return null;
        }

        for (int i = 0; i < customButtons.Length; i++)
        {
            if (customButtons[i].IsBack && customButtons[i].IsInteractable)
            {
                return customButtons[i];
            }
        }

        return null;
    }

    private void SubscribeCallbacks() //Custom
    {
        inputMapping.InUI.Submit.performed += OnSubmit;
        inputMapping.InUI.Back.performed += OnBack;
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
        inputMapping.InUI.Back.performed -= OnBack;
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
                Log("Enabled" + actionMap.name);
            }
        }

        EnableUIInputs();
        ResetSelectedButtons();
    }

    public static void DisableControlSchemes()
    {
        foreach (var actionMap in GetInputMapping.asset.actionMaps)
        {
            if (actionMap.enabled)
            {
                actionMap.Disable();
                Log("Disabled" + actionMap.name);
            }
        }
    }
    #endregion

    #region Enable/Disable UI Events
    public static void DisableUIInputs()
    {
        current.recieveUIInput = false;
        Log("UI Inputs are Disabled");
    }
    public static void EnableUIInputs()
    {
        current.recieveUIInput = true;
        Log("UI Inputs are Enabled");
    }
    public static void ResetSelectedButtons()
    {
        if (selectedButton != null)
        {
            selectedButton.ForceNoHoverLogic();
            selectedButton = null;
        }
        if (hoveredButton != null)
        {
            hoveredButton.ForceNoHoverLogic();
            hoveredButton = null;
        }

        current.defaultButton = GetDefaultButton();
        current.backButton = GetBackButton();

        //Hover Default button if enabled
        if (current.startWithDefaultHovered && current.defaultButton != null)
        {
            current.defaultButton.HoverLogic();
        }
    }
    public static void DehoverAll()
    {
        allNoHover.Invoke();
        LogWarning("All buttons DeHovered!");
    }
    #endregion

    public static void UpdateSelectedButton(CustomButton newSelect)
    {
        if (newSelect == selectedButton)
            return;

        if (newSelect.state != ButtonState.Selected)
        {
            LogWarning("Selected button was not in the selected state!");
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

    #region Common Triggers
    public void OnSubmit(InputAction.CallbackContext context)
    {
        //Return if null
        if (hoveredButton == null)
        {
            LogWarning("No hovered button found.");
            return;
        }

        //Return if button is not interactable or the eventsystem is disabled
        if (!hoveredButton.IsInteractable || !InputEnabled)
        {
            LogWarning("Hovered button is not interactable or Input is disabled.");
            return;
        }

        hoveredButton.ClickLogic();
    }

    public void OnBack(InputAction.CallbackContext context)
    {
        //Return if null
        if (backButton == null)
        {
            LogWarning("No back button found.");
            return;
        }

        //Return if button is not interactable or the eventsystem is disabled
        if (!backButton.IsInteractable || !InputEnabled)
        {
            LogWarning("Back button is not interactable or Input is disabled.");
            return;
        }

        backButton.HoverLogic();
        backButton.ClickLogic();
    }
    #endregion

    #region Navigation
    private void TriggerNavigation(InputAction.CallbackContext context)
    {
        if (activeButton == null)
            return;

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
        yield return new WaitForSecondsRealtime(firstClickDelay);

        while (true)
        {
            Navigate(direction);

            yield return new WaitForSecondsRealtime(minimumClickDelay);
        }
    }

    public void Navigate(NavigateDirections direction)
    {
        CustomButton nextButton = GetNextActiveButton(activeButton, direction);

        if (nextButton == null)
            return;

        nextButton.HoverLogic();
    }

    private CustomButton GetNextActiveButton(CustomButton currentButton, NavigateDirections direction, int recursionCount = 0)
    {
        CustomButton nextButton = null;
        switch (direction)
        {
            case NavigateDirections.Up:
                nextButton = currentButton.navigation.up;
                break;
            case NavigateDirections.Down:
                nextButton = currentButton.navigation.down;
                break;
            case NavigateDirections.Left:
                nextButton = currentButton.navigation.left;
                break;
            case NavigateDirections.Right:
                nextButton = currentButton.navigation.right;
                break;
            default:
                break;
        }

        if (nextButton == null || recursionCount > 100)
            return null;

        return nextButton.IsInteractable ? nextButton : GetNextActiveButton(nextButton, direction, ++recursionCount);
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

    #region Debug
    private static void Log(object message)
    {
        if (current.debug)
            Debug.Log(message);
    }
    private static void LogWarning(object message)
    {
        if (current.debug)
            Debug.LogWarning(message);
    }
    private static void LogError(object message)
    {
        if (current.debug)
            Debug.LogError(message);
    }
    #endregion
}
