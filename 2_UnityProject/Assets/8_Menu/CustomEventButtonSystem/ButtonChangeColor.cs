using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class ButtonChangeColor : CustomButtonFunctionality
{
    private TextMeshProUGUI textToChange;

    [SerializeField]
    private Color buttonColorNormal = Color.gray;
    [SerializeField]
    private Color buttonColorHighlighted = Color.white;
    [SerializeField]
    private Color buttonColorSelected = Color.yellow;

    protected override void OnAwake()
    {
        textToChange = GetComponentInChildren<TextMeshProUGUI>();
        textToChange.color = buttonColorNormal;

        AddFunctionToEvent(ChangeColorHighlighted, ButtonEventType.Hover);
        AddFunctionToEvent(ChangeColorNormal, ButtonEventType.NoHover);
        AddFunctionToEvent(ChangeColorSelected, ButtonEventType.Click);
    }

    private void ChangeColorNormal()
    {
        textToChange.color = buttonColorNormal;
    }
    private void ChangeColorHighlighted()
    {
        textToChange.color = buttonColorHighlighted;
    }
    private void ChangeColorSelected()
    {
        textToChange.color = buttonColorSelected;
    }
}
