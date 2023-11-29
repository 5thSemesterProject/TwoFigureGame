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

    void  OnValidate()
    {
        if (TryGetComponent(out SphereCollider sphereCollider))
        {
            sphereCollider.radius = smokeIntersectionRadius;
        }   
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

    public int GetAmountOfCharacters()
    {
        return amountOfCharacters;
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position,smokeIntersectionRadius);
        
    }

    void  OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Movement movementComp))
        {
            movementComp.oxygenstation = this;
            amountOfCharacters++;
        }
    }

    void  OnTriggerStay(Collider other)
    {   
        if (other.TryGetComponent(out Movement movementComp))
        {
            movementComp.oxygenstation = this;
        }
    }

    void  OnTriggerExit(Collider other)
    {   
        if (other.TryGetComponent(out Movement movementComp))
        {
            movementComp.oxygenstation = null;
            amountOfCharacters--;
        }
    }
}
