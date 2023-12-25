using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
struct VoiceLineDataContainer
{
   public VoicelineData maleData, femaleData;

   public void Initalize()
   {
        maleData.characterType = CharacterType.Man;
        femaleData.characterType = CharacterType.Woman;
   }

   public E_1_Voicelines GetVoicelineByCharacterType(CharacterType characterType)
   {
      if (characterType == CharacterType.Man)
          return maleData.voiceline;
     else if (characterType == CharacterType.Woman)
          return femaleData.voiceline;
     else
          return default;
   }

}

[Serializable]
struct VoicelineData
{
    public E_1_Voicelines voiceline;
    [HideInInspector]public CharacterType characterType;
}

public class CustomCharacterSounds : MonoBehaviour
{
    [SerializeField] VoiceLineDataContainer lowHealthSound;
    [SerializeField] float waitTimeBetweenClips = 1;
    
    void Start()
    {
        lowHealthSound.Initalize();
        CustomEvents.lowOxygen+= PlayLowHealthSound;
    }

   void PlayLowHealthSound(CharacterData characterData)
   {
        E_1_Voicelines voicelineToPlay = lowHealthSound.GetVoicelineByCharacterType(characterData.movement.characterType);
        VoicelinePlayer.instance.LoadAndPlayVoiceLine(voicelineToPlay,waitTimeBetweenClips,2);
   }
}
