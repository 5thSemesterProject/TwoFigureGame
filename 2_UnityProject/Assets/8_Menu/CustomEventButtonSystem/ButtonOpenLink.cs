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
            URL = RandomLink();
        }
        Application.OpenURL(URL);
    }

    private string RandomLink()
    {
        string[] links = new string[]
        {
            "https://pin.it/204aGVT",
            "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
        };

        return links[Random.Range(0, links.Length)];
    }
}
