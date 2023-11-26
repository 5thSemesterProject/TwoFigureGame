using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomEvents : MonoBehaviour
{
    public delegate void CharacterSwitchEvent(GameObject activeCharacter);
    public static event CharacterSwitchEvent characterSwitch;

    public static void RaiseCharacterSwitch(GameObject activeCharacter)
    {
        characterSwitch?.Invoke(activeCharacter);
    }
}
