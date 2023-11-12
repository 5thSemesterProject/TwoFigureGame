using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oxygenstation : MonoBehaviour
{
   OxygenData oxygenData;
   float chargeRate = 5.0f;

    void  Awake()
    {
        new OxygenData(200,0.1f);
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
}
