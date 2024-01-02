using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;


[Serializable]
struct VoiceLineDataContainer
{
     public bool randomize;
   public VoicelineData maleData, femaleData;
   public void Initalize()
   {
        maleData.characterType = CharacterType.Man;
        femaleData.characterType = CharacterType.Woman;
   }

   public E_1_Voicelines GetVoicelineByCharacterType(CharacterType characterType)
   {
      if (characterType == CharacterType.Man)
      {
          if (randomize)
               return maleData.GetRandomVoiceline();
          else
               return maleData.voicelines[0];     
      }
          
      if (characterType == CharacterType.Man)
      {
          if (randomize)
               return femaleData.GetRandomVoiceline();
          else
               return femaleData.voicelines[0];     
      }

      return default;
   }
}

[Serializable]
struct VoicelineData
{
    public E_1_Voicelines[] voicelines;
    [HideInInspector]public int lastRandom;
    [HideInInspector]public CharacterType characterType;

    public E_1_Voicelines GetRandomVoiceline()
    {
          return voicelines[AudioUtility.RandomNumber(lastRandom,voicelines.Length,out lastRandom)];
    }
}

public class CustomCharacterSounds : MonoBehaviour
{
    //[SerializeField] VoiceLineDataContainer lowHealthSound;
    [SerializeField] VoiceLineDataContainer chargingOxygenSound;
    [SerializeField] float waitTimeBetweenClips = 1;
    
    void Start()
    {
        //lowHealthSound.Initalize();
        //CustomEvents.lowOxygen+= PlayLowHealthSound;
        CustomEvents.chargingOxygen+=PlayChargingOxygenSound;
    }

   /*void PlayLowHealthSound(CharacterData characterData)
   {
        E_1_Voicelines voicelineToPlay = lowHealthSound.GetVoicelineByCharacterType(characterData.movement.characterType);
        VoicelinePlayer.instance.TryPlayVoiceLine(voicelineToPlay,waitTimeBetweenClips,2);
   }*/

   void PlayChargingOxygenSound(CharacterData characterData)
   {
        E_1_Voicelines voicelineToPlay = chargingOxygenSound.GetVoicelineByCharacterType(characterData.movement.characterType);
        VoicelinePlayer.instance.TryPlayVoiceLine(voicelineToPlay,waitTimeBetweenClips,2);
   }
}
