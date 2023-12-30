using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

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

public enum ButtonClickType
{
    Select,
    Click
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

    public CustomButton[] GetAsArray()
    {
        return new CustomButton[4]
        {
            up,
            right,
            down,
            left,
        };
    }
}

public delegate void ButtonEvent();
#endregion

#region Editor
#if UNITY_EDITOR
[CustomEditor(typeof(CustomButton))]
public class CustomButtonEditor : Editor
{
    private bool isVisualized;

    private void OnEnable()
    {
        SceneView.duringSceneGui += SceneGUI;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Visualize"))
            isVisualized = !isVisualized;

        //if (GUILayout.Button("Generate"))
        //    GenerateNavigation();
    }

    private void SceneGUI(SceneView sceneView)
    {
        if (isVisualized)
            VisualizeNavigation();
    }

    private void VisualizeNavigation()
    {
        CustomButton self = (CustomButton)target;
        NavigationButtons navigations = self.navigation;

        foreach (var button in navigations.GetAsArray())
        {
            if (button != null)
                DrawArrow(self.gameObject.transform.position, button.gameObject.transform.position);
        }
    }

    //private void GenerateNavigation()
    //{

    //}

    private void DrawArrow(Vector3 startPos, Vector3 endPos)
    {
        Handles.color = Color.green;

        Vector3 direction = endPos - startPos;
        float arrowSize = 1f;
        float arrowAngle = 20f;
        float arrowThickness = 3;

        Handles.DrawLine(startPos, endPos, arrowThickness);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowAngle, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowAngle, 0) * Vector3.forward;
        float distance = direction.magnitude;

        Handles.DrawLine(endPos, endPos + right * arrowSize * distance * 0.2f, arrowThickness);
        Handles.DrawLine(endPos, endPos + left * arrowSize * distance * 0.2f, arrowThickness);
    }
}
#endif
#endregion

#region Custom Button Core
public class CustomButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    //Events
    public event ButtonEvent clickEvent;
    public event ButtonEvent hoverEvent;
    public event ButtonEvent hoverEndEvent;

    //Public Attributes
    [Header("Debug")]
    [Space]
    public ButtonState state;
    public bool holdsPointer = false;

    [Header("Settings")]
    [Space]
    [SerializeField] private ButtonClickType ButtonClickType = ButtonClickType.Click;
    [SerializeField] private bool isDefaultButton = false;
    [SerializeField] private bool isBackButton = false;
    [SerializeField] private bool interactable = true;
    public bool IsDefault { get => isDefaultButton; }
    public bool IsBack { get => isBackButton; }
    public bool IsInteractable
    {
        get
        {
            ButtonEnabler enabler = GetComponentInParent<ButtonEnabler>();
            if (enabler == null)
            {
                return interactable;
            }
            return interactable && enabler.interactable;
        }
    }

    //Navigation
    [Header("Navigation")]
    [Space]
    public NavigationButtons navigation = new NavigationButtons();

    #region Default Logic
    private void OnEnable()
    {
        CustomEventSystem.allNoHover += NoHoverLogic;
    }
    private void OnDestroy()
    {
        CustomEventSystem.allNoHover -= NoHoverLogic;
    }

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
        if (!IsInteractable || !CustomEventSystem.InputEnabled)
            return;

        if (ButtonClickType == ButtonClickType.Select)
        {
            state = ButtonState.Selected;
            CustomEventSystem.UpdateSelectedButton(this);
        }

        RaiseClick();
    }
    public void HoverLogic()
    {
        //Return if button is not interactable or the eventsystem is disabled
        if (!IsInteractable || !CustomEventSystem.InputEnabled)
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
        if (state == ButtonState.Selected)
            return;

        ForceNoHoverLogic();
    }
    public void ForceNoHoverLogic()
    {
        if (CustomEventSystem.hoveredButton == this)
            CustomEventSystem.hoveredButton = null;

        state = ButtonState.None;
        RaiseHoverEnd();
    }
    #endregion
}
#endregion

#region Component Base Class
[ExecuteInEditMode]
[RequireComponent(typeof(CustomButton))]
public class CustomButtonFunctionality : MonoBehaviour
{
    protected CustomButton button;

    #region Awake
    private void Awake()
    {
        button = GetComponent<CustomButton>();
        if (Application.isPlaying)
            OnAwake();
        else
            OnAwakeEditor();
        OnAwakeAlways();
    }

    //To be overridden by children to access the awake method
    protected virtual void OnAwake()
    {

    }
    //To be overridden by children to access the awake method
    protected virtual void OnAwakeAlways()
    {

    }
    //To be overridden by children to access the awake method
    protected virtual void OnAwakeEditor()
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
#endregion