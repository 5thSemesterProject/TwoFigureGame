using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using UnityEditor;

public enum ColorChangeType
{
    Text,
    Image,
    Both
}

public class ButtonChangeColor : CustomButtonFunctionality
{
    [Header("To Change")]
    [SerializeField] private TextMeshProUGUI[] textToChange;
    [SerializeField] private Image[] imageToChange;
    [SerializeField] ColorChangeType ToChangeType = ColorChangeType.Both;

    [Space]
    [Header("Colors")]
    [SerializeField] private Color buttonColorNormal = Color.gray;
    [SerializeField] private Color buttonColorHighlighted = Color.white;
    [SerializeField] private Color buttonColorSelected = Color.yellow;
    [SerializeField] private ButtonState stateToDisplay = ButtonState.None;

    #region Editor Stuff
    private void OnValidate()
    {
        switch (stateToDisplay)
        {
            case ButtonState.None:
                ChangeColorNormal();
                break;
            case ButtonState.Hovered:
                ChangeColorHighlighted();
                break;
            case ButtonState.Selected:
                ChangeColorSelected();
                break;
        }
    }

    private void OnEnable()
    {
        switch (stateToDisplay)
        {
            case ButtonState.None:
                ChangeColorNormal();
                break;
            case ButtonState.Hovered:
                ChangeColorHighlighted();
                break;
            case ButtonState.Selected:
                ChangeColorSelected();
                break;
        }
    }
    #endregion

    protected override void OnAwakeAlways()
    {
        if (textToChange == null || textToChange.Length <= 0)
        {
            textToChange = GetComponentsInChildren<TextMeshProUGUI>();
        }

        if (imageToChange == null || imageToChange.Length <= 0)
        {
            imageToChange = GetComponentsInChildren<Image>();
        }

        ChangeColorNormal();

        AddFunctionToEvent(ChangeColorHighlighted, ButtonEventType.Hover);
        AddFunctionToEvent(ChangeColorNormal, ButtonEventType.NoHover);
        AddFunctionToEvent(ChangeColorSelected, ButtonEventType.Click);
    }

    private void ChangeColorNormal()
    {
        if (textToChange != null && ToChangeType != ColorChangeType.Image)
            for (int i = 0; i < textToChange.Length; i++)
            {
                textToChange[i].color = buttonColorNormal;
            }
        if (imageToChange != null && ToChangeType != ColorChangeType.Text)
            for (int i = 0; i < imageToChange.Length; i++)
            {
                imageToChange[i].color = buttonColorNormal;
            }
    }
    private void ChangeColorHighlighted()
    {
        if (textToChange != null && ToChangeType != ColorChangeType.Image)
            for (int i = 0; i < textToChange.Length; i++)
            {
                textToChange[i].color = buttonColorHighlighted;
            }
        if (imageToChange != null && ToChangeType != ColorChangeType.Text)
            for (int i = 0; i < imageToChange.Length; i++)
            {
                imageToChange[i].color = buttonColorHighlighted;
            }
    }
    private void ChangeColorSelected()
    {
        if (textToChange != null && ToChangeType != ColorChangeType.Image)
            for (int i = 0; i < textToChange.Length; i++)
            {
                textToChange[i].color = buttonColorSelected;
            }
        if (imageToChange != null && ToChangeType != ColorChangeType.Text)
            for (int i = 0; i < imageToChange.Length; i++)
            {
                imageToChange[i].color = buttonColorSelected;
            }
    }
}
