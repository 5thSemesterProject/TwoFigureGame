using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomEvents : MonoBehaviour
{   

    public delegate void CharacterDataDel(CharacterData characterData);

    //Character Switch
    public delegate void CharacterSwitchEvent(GameObject activeCharacter);
    public static event CharacterSwitchEvent characterSwitch;

    public static void RaiseCharacterSwitch(GameObject activeCharacter)
    {
        characterSwitch?.Invoke(activeCharacter);
    }

    //Voiceline Trigger
    #region  Voiceline Trigger
    public static event CharacterDataDel lowOxygen;
    public static event CharacterDataDel chargingOxygen;

    public static void RaiseLowOxygen(CharacterData characterData)
    {
        lowOxygen?.Invoke(characterData);
    }

   
    public static void RaiseChargingOxygen(CharacterData characterData)
    {
        chargingOxygen?.Invoke(characterData);
    }
    #endregion

    //Unlocked Twine Story
    public delegate void TwineStoryDel (CharacterType characterType,TwineStoryData twineStoryData);
    public static event TwineStoryDel unlockedTwineStory;

    public static void RaiseUnlockTwineStory(CharacterType characterType, TwineStoryData twineStoryData)
    {
        unlockedTwineStory?.Invoke(characterType,twineStoryData);
    }

}

