using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
struct VoiceLineDataContainer
{
   public VoicelineData maleData, femaleData;

   public void Initalize()
   {
        maleData.characterType = CharacterType.Man;
        femaleData.characterType = CharacterType.Woman;
   }

}

[Serializable]
struct VoicelineData
{
    public E_1_Voicelines voiceline;
    [HideInInspector]public CharacterType characterType;
}

public class CustomVoiceTriggerData : MonoBehaviour
{
    [SerializeField] VoiceLineDataContainer onLowHealth;

   void  Awake()
   {
        onLowHealth.Initalize();

   }

   void PlayVoiceLine()
   {
        //Load and play voiceline
   }

}
