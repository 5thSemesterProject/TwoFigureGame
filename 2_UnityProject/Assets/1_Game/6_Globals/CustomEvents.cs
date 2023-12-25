using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomEvents : MonoBehaviour
{   
    //Character Switch
    public delegate void CharacterSwitchEvent(GameObject activeCharacter);
    public static event CharacterSwitchEvent characterSwitch;

    public static void RaiseCharacterSwitch(GameObject activeCharacter)
    {
        characterSwitch?.Invoke(activeCharacter);
    }

    //Voiceline Trigger
    public delegate void TriggerVoicelineEvent(CharacterData characterData);
    public static event TriggerVoicelineEvent lowOxygen;
    public static event TriggerVoicelineEvent chargingOxygen;

    public static void RaiseLowOxygen(CharacterData characterData)
    {
        lowOxygen?.Invoke(characterData);
    }

   
    public static void RaiseChargingOxygen(CharacterData characterData)
    {
        chargingOxygen?.Invoke(characterData);
    }
}
