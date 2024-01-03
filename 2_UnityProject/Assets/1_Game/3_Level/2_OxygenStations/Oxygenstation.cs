using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using UnityEngine;
using UnityEngine.VFX;

public class Oxygenstation : MonoBehaviour, IIntersectGas
{
   OxygenData oxygenData;
   [SerializeField]float chargeRate = 5.0f;
   [SerializeField] float smokeIntersectionRadius;
   Material fluidMaterial;
   [SerializeField]float maxAlpha = 20;
    [SerializeField]float maxEmission = 20;
   [SerializeField] VisualEffect absorbEffect1;
   [SerializeField] VisualEffect absorbEffect2;
   float targetEmission;
   float initialEmission = 0;
   Coroutine emissionCoroutine;
   bool isCharging;
   int amountOfCharacters;
   float maxSmokeIntersectionRadus;

    bool alphaIncrease;
   Coroutine alphaCoroutine;

    #region  Setup
    void Start()
    {
        oxygenData = GameStats.instance.oxygenStation;
        oxygenData.currentOxygen =oxygenData.maxOxygen;
        gameObject.layer = 3;

        Material[] materials = GetComponent<Renderer>().materials;

        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i].name.Contains("Oxygen"))
                fluidMaterial = materials[i];
        }

        maxSmokeIntersectionRadus = smokeIntersectionRadius;

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
        SetAlpha(0);

        
    }

    void  OnValidate()
    {
        if (TryGetComponent(out SphereCollider sphereCollider))
        {
            sphereCollider.radius = smokeIntersectionRadius;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position,smokeIntersectionRadius);
        
    }
    #endregion


    public float ChargePlayer()
    {

        if (oxygenData.currentOxygen>0)
        {
            isCharging = true;
            alphaIncrease = true;
            oxygenData.currentOxygen-=chargeRate*Time.deltaTime;
            SetFluidLevel();
            LerpEmission();
            SetAbsorbVFX();
            return chargeRate*Time.deltaTime;
        }
        
        return 0;
    }

    
    #region SetAbsorbVFX
    void SetAbsorbVFX()
    {
        if (absorbEffect1 && absorbEffect2)
        {
            if (alphaCoroutine==null)
                alphaCoroutine = StartCoroutine(_AbsorbAlpha());
        }
    }

    IEnumerator _AbsorbAlpha()
    {
        float currentAlpha = GetAlpha();
        float smoothTime = 0.5f;
        float velocity = 0;

        while (true)
        {
            float targetAlpha = alphaIncrease?1:0;
            currentAlpha = Mathf.SmoothDamp(currentAlpha, targetAlpha, ref velocity, smoothTime);

            if (currentAlpha<0.01f)
            {
                currentAlpha = targetAlpha;
                SetAlpha(currentAlpha);
                break;
            }

            SetAlpha(currentAlpha);
            alphaIncrease = false;

            yield return null;
        }

        alphaCoroutine = null; 
    }

    public void SetAlpha(float inputAlpha)
    {

        absorbEffect1.SetFloat("Alpha",inputAlpha);
        absorbEffect1.SetFloat("SpawnRate",inputAlpha);

        absorbEffect2.SetFloat("Alpha",inputAlpha);
        absorbEffect2.SetFloat("SpawnRate",inputAlpha);
    }

    public float GetAlpha()
    {
        return absorbEffect1.GetFloat("SpawnRate");
    }
    #endregion

    #region Smoke Intersection Radius

    public float GetIntersectionRadius()
    {
        return smokeIntersectionRadius*9;
    }
    #endregion

    void SetFluidLevel()
    {
        float fluidLevel = (oxygenData.maxOxygen-oxygenData.currentOxygen)/oxygenData.maxOxygen;
        fluidMaterial.SetFloat("_Cutoff",fluidLevel);
    }

    #region  CountCharacters
    public int GetAmountOfCharacters()
    {
        return amountOfCharacters;
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
            //movementComp.oxygenstation = this;
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

    #endregion


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
