using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Net.WebRequestMethods;

public class ButtonOpenLink : CustomButtonFunctionality
{
    [SerializeField]
    private ButtonEventType activeTriggerMoment = ButtonEventType.Click;

    [SerializeField]
    private string URL;

    protected override void OnAwake()
    {
        AddFunctionToEvent(OpenLink, activeTriggerMoment);
    }

    private void OpenLink()
    {
        if (URL == null || string.IsNullOrEmpty(URL))
        {
            URL = "https://pin.it/204aGVT";
        }
        Application.OpenURL(URL);
    }
}
