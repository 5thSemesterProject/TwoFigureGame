using System.Collections;
using System.Collections.Generic;
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
}

public class GameStats : MonoBehaviour
{
   public static GameStats instance;

    public float inactiveFollowDistance = 4;
    public OxygenData characterOxy,oxygenStation;


   void  Awake()
   {
        if (instance==null)
            instance = this;
        else
            Destroy(this);
   }

}
