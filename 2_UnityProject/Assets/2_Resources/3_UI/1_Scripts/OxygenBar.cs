using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OxygenBar : MonoBehaviour
{
    Slider slider;

    void Awake()
    {
        slider = GetComponentInChildren<Slider>();
    }

    public void SetValue(float value)
    {
        slider.value = value;
    }

}
