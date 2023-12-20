using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oxygenstation : MonoBehaviour, IIntersectSmoke
{
   OxygenData oxygenData;
   [SerializeField]float chargeRate = 5.0f;
   [SerializeField] float smokeIntersectionRadius;
   [SerializeField] Material fluidMaterial;

    [SerializeField]float maxEmission = 20;
   float targetEmission;
   float initialEmission = 0;
   Coroutine emissionCoroutine;

   bool isCharging;

   int amountOfCharacters;

    void  Awake()
    {
        oxygenData = new OxygenData(200,0.1f);

        gameObject.layer = 3;

        if (TryGetComponent(out SphereCollider sphereCollider))
        {
            sphereCollider.radius = smokeIntersectionRadius;
            sphereCollider.includeLayers = Physics.AllLayers;
        }
        else
        {
           Debug.LogWarning("Missing Sphere Collider. Pls add a sphere collider with on trigger on "+gameObject.name);
        }

        initialEmission = GetEmission();
        SetFluidLevel();
        
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
            isCharging = true;
            oxygenData.currentOxygen-=chargeRate*Time.deltaTime;
            SetFluidLevel();
            LerpEmission();
            return chargeRate*Time.deltaTime;
        }
        
        return 0;
    }

    void SetFluidLevel()
    {
        float fluidLevel = (oxygenData.maxOxygen-oxygenData.currentOxygen)/oxygenData.maxOxygen;
        fluidMaterial.SetFloat("_Cutoff",fluidLevel);
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


    #region Emission Handling

    public void LerpEmission()
    {
        if (emissionCoroutine == null)
        {
            emissionCoroutine = StartCoroutine(_LerpEmission());
        }
  
    }
    IEnumerator _LerpEmission()
    {
        float currentEmission = GetEmission();
        float smoothTime = 0.33f;
        float velocity = 0;

        while (true)
        {
            targetEmission = isCharging?maxEmission:initialEmission;

            currentEmission = Mathf.SmoothDamp(currentEmission, targetEmission, ref velocity, smoothTime);

            if (currentEmission<=initialEmission)
            {
                currentEmission = targetEmission;
                SetEmission(currentEmission);
                break;
            }

            SetEmission(currentEmission);
            isCharging = false;

            yield return null;
        }

        emissionCoroutine = null; 
    }

    public void SetEmission(float inputAlpha)
    {

        fluidMaterial.SetFloat("_Emission",inputAlpha);
    }

    public float GetEmission()
    {
        return fluidMaterial.GetFloat("_Emission");
    }
    #endregion

}