using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomBlurAndFade : ButtonGroupFade
{
    [SerializeField] private Blur blur;

    protected override void CustomLogic(float timeElapsed, float currentValue)
    {
        blur.SetBlur(Mathf.Lerp(0, 3, currentValue));
    }
}
