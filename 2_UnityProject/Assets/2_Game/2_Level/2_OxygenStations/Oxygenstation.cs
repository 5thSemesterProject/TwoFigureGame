using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oxygenstation : MonoBehaviour, IIntersectSmoke
{
   OxygenData oxygenData;
   [SerializeField]float chargeRate = 5.0f;
   [SerializeField] float smokeIntersectionRadius;


   int amountOfCharacters;

    void  Awake()
    {
        oxygenData = new OxygenData(200,0.1f);
    }

    public float ChargePlayer()
    {
        if (oxygenData.currentOxygen>0)
        {
            oxygenData.currentOxygen-=chargeRate*Time.deltaTime;
            return chargeRate*Time.deltaTime;
        }
        
        return 0;

    }

    public float GetIntersectionRadius()
    {
        return smokeIntersectionRadius;
    }

    public void AddCharacter()
    {
        amountOfCharacters++;
    }

    public int GetAmountOfCharacters()
    {
        return amountOfCharacters;
    }

    public void RemoveCharacter()
    {
        amountOfCharacters--;
    }
}
