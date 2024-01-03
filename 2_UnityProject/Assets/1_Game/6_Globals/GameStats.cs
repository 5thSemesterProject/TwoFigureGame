using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public struct OxygenData
{
    public float maxOxygen;
    [HideInInspector]public float currentOxygen;
    public float fallOfRate;

    public OxygenData(float maxOxygen, float fallOfRate)
    {
        this.maxOxygen = maxOxygen;
        currentOxygen = maxOxygen;
        this.fallOfRate = fallOfRate;
    }

    public void FallOff()
    {
        currentOxygen -= fallOfRate * Time.deltaTime;
    }

    public void InreaseOxygen(float amount)
    {
        if (currentOxygen <= maxOxygen)
            currentOxygen += amount;
    }

    public bool IsLow
    {
        get
        {
            return currentOxygen <= maxOxygen / 5;
        }
    }
}

[System.Serializable]
public struct CharacterOxygenData
{
   public OxygenData oxygenData;
   public float lowOxygenThreshhold;
   public float fallOffTimeInMinutes;
   public AnimationCurve fallOffCurve;
   public float maxFallOff;
   [HideInInspector]public float initialFallOff;

    public void Initialize()
    {
        initialFallOff = oxygenData.fallOfRate;
        oxygenData.currentOxygen = oxygenData.maxOxygen;
    }
   public void UpdateFallOff(float elapsedTime)
   {
        float fallOffProgress = elapsedTime/(fallOffTimeInMinutes*100*60);
        oxygenData.fallOfRate = Mathf.Lerp(oxygenData.fallOfRate,maxFallOff,fallOffCurve.Evaluate(fallOffProgress));
   }
}

public class GameStats : MonoBehaviour
{
   public static GameStats instance;
    public float inactiveFollowDistance = 4;
    public OxygenData oxygenStation;
    public CharacterOxygenData characterOxy;

   void Awake()
   {    
        if (instance==null)
            instance = this;
        else
            Destroy(this);

        characterOxy.Initialize();
   }



}
