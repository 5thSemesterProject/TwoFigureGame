using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;


public class VolumetricFogHandler : MonoBehaviour
{   
    [SerializeField] LayerMask layerMask;

    [Header("Density")]
    [SerializeField] float minDensity = 0.01f, maxDensity = 0.11f;

    [Header("SmokeMasks")]
    List<Transform> intersectSmokeTransforms  = new List<Transform>();
    int maxMasks = 6;

    int maxRectangles = 8;

    LocalVolumetricFog localVolumetricFog;

    void Start()
    {   
        localVolumetricFog = GetComponent<LocalVolumetricFog>();
        ResetSmokeMasks();
    }

    void LateUpdate()
    {
        //Update Epicenter
        float fallOffValue = Mathf.Lerp(minDensity,maxDensity,CharacterOxygenData.falloffProgress);
        localVolumetricFog.parameters.materialMask.SetFloat($"_FalloffFactor",fallOffValue);
        UpdateSpheres(intersectSmokeTransforms.ToArray());
        CheckColliders();
    }

    void UpdateSpheres(Transform [] transforms) 
    {
        List<Vector4> sphereInfos  = new List<Vector4>();
        List<Vector4> rectangleInfos = new List<Vector4>();
        var isPlayer = new List<bool>();
        foreach(Transform transformItem in transforms)
        {
            float radius = 2;

            bool _isPlayer = false;
            
            //Get all monobehvaiours
            MonoBehaviour[] allMonoBehaviours = transformItem.GetComponents<MonoBehaviour>();
            for (int i = 0; i < allMonoBehaviours.Length; i++)
            {
                if (allMonoBehaviours[i] is IIntersectGas)
                {
                    IIntersectGas component = allMonoBehaviours[i] as IIntersectGas;
                    radius = component.GetIntersectionRadius();

                    if (allMonoBehaviours[i] is Movement)
                        _isPlayer = true;

                    isPlayer.Add(_isPlayer);        
                    sphereInfos.Add(VectorHelper.Convert3To4(transformItem.position,radius));
                }
                else if (allMonoBehaviours[i] is ExcludeRectangle)
                {
                   ExcludeRectangle excludeRectangle =  allMonoBehaviours[i] as ExcludeRectangle;
                   rectangleInfos.Add(excludeRectangle.GetRectangleData());
                }

            }
        }
        
        SetSpheres(PutPlayersToFront(sphereInfos.ToArray(),isPlayer.ToArray()));
        SetExcludeRectangles(rectangleInfos.ToArray());
    }

    void SetSpheres(Vector4[] sphereInfos)
    {
        for (int i =0;i<sphereInfos.Length;i++)
        {   
            localVolumetricFog.parameters.materialMask.SetVector($"_Sphere0{i+1}", sphereInfos[i]);   
        }
    }

    void SetExcludeRectangles(Vector4[] rectangleInfos)
    {
         for (int i =0;i<rectangleInfos.Length;i++)
        {   
            localVolumetricFog.parameters.materialMask.SetVector($"_ExcludeRectangle_0{i+1}", rectangleInfos[i]);   
        }
    }

    void AddSmokeMask(Transform transform)
    {
        if (intersectSmokeTransforms.Count<maxMasks+maxRectangles)
            intersectSmokeTransforms.Add(transform);
        else
            Debug.Log("There are more objects that mask out smoked then previsouly set up");
    }

    void DeleteSmokeMask(Transform transform)
    {
        intersectSmokeTransforms.Remove(transform);
    }

    void ResetSmokeMasks()
    {
        intersectSmokeTransforms.Clear();
        ResetSpheres();
    }

    void ResetSpheres()
    {
        //Reset Sphere Values in Material
        Vector4[] emptySpheres = new Vector4[maxMasks];
        for (int i = 0; i < maxMasks; i++)
        {
            emptySpheres[i] = Vector4.zero;
        }
        SetSpheres(emptySpheres);
    }

    void ResetRectangles()
    {
        //Reset Sphere Values in Material
        Vector4[] emptyRectangles = new Vector4[maxRectangles];
        for (int i = 0; i < maxMasks; i++)
        {
            emptyRectangles[i] = Vector4.zero;
        }
        SetExcludeRectangles(emptyRectangles);
    }

    void CheckColliders()
    {   
        Vector3 colliderSize = localVolumetricFog.parameters.size;
        Collider[] hitColliders = Physics.OverlapBox(transform.position, colliderSize/2+Vector3.one, Quaternion.identity, layerMask);

        //Check if object is already in list
        for (int i = 0; i < hitColliders.Length; i++)
        {   
            if (!intersectSmokeTransforms.Contains(hitColliders[i].transform))
            {
                AddSmokeMask(hitColliders[i].transform);
            }
        }

        //Check if object in list moved out
        List<Transform> hitColliderTransforms = new List<Transform>();
        foreach(Collider hitCollider in hitColliders)
        {
            hitColliderTransforms.Add(hitCollider.transform);
        }

        for (int i = 0; i < intersectSmokeTransforms.Count; i++)
        {   
            if (!hitColliderTransforms.Contains(intersectSmokeTransforms[i]))
            {
                DeleteSmokeMask(intersectSmokeTransforms[i]);
            }
        }

        ResetSpheres();
        UpdateSpheres(intersectSmokeTransforms.ToArray());
    }

    public Vector4[] PutPlayersToFront(Vector4[] sphereInfos, bool[] isPlayer)
    {
        var output = new List<Vector4>();

        //Add players first
        for (int i = 0; i < sphereInfos.Length; i++)
        {
            if (isPlayer[i])
                output.Add(sphereInfos[i]);
        }

        for (int i = 0; i < sphereInfos.Length; i++)
        {
            if (!isPlayer[i])
                output.Add(sphereInfos[i]);
        }
        

        return output.ToArray();
    }

     private void OnApplicationQuit()
    {
        ResetSpheres();
        ResetRectangles();
        //Reset FallOffFactor
        localVolumetricFog.parameters.materialMask.SetFloat($"_FalloffFactor",minDensity);
    }

}
