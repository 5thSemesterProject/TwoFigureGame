using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class VolumetricFogHandler : MonoBehaviour
{   
    [SerializeField] LayerMask layerMask;
    [SerializeField] Transform fallOffEpicenter;
    List<Transform> intersectSmokeTransforms  = new List<Transform>();
    int maxMasks = 6;

    LocalVolumetricFog localVolumetricFog;

    void Start()
    {   
        localVolumetricFog = GetComponent<LocalVolumetricFog>();
        ResetSmokeMasks();
    }

    void  OnDrawGizmos()
    {
        //Update Epicenter
        //if (fallOffEpicenter!=null)
           //localVolumetricFog.parameters.materialMask.SetVector($"_FallOffEpicenter", new Vector4(0,0,0,0));
        
    }

    void LateUpdate()
    {
        //Update Epicenter
        localVolumetricFog.parameters.materialMask.SetVector($"_FallOffEpicenter", fallOffEpicenter.transform.position);

        UpdateSpheres(intersectSmokeTransforms.ToArray());
        CheckColliders();
    }

    void UpdateSpheres(Transform [] transforms) 
    {
        List<Vector4> sphereInfos  = new List<Vector4>();
        foreach(Transform transformItem in transforms)
        {
            float radius = 2;
            
            //Get all monobehvaiours
            MonoBehaviour[] allMonoBehaviours = transformItem.GetComponents<MonoBehaviour>();
            for (int i = 0; i < allMonoBehaviours.Length; i++)
            {
                if (allMonoBehaviours[i] is IIntersectSmoke)
                {
                    IIntersectSmoke component = allMonoBehaviours[i] as IIntersectSmoke;
                    radius = component.GetIntersectionRadius();
                }
            }
                
            sphereInfos.Add(VectorHelper.Convert3To4(transformItem.position,radius));
        }
        SetSpheres(sphereInfos.ToArray());
    }

    void SetSpheres(Vector4[] sphereInfos)
    {
        for (int i =0;i<sphereInfos.Length;i++)
        {   
            localVolumetricFog.parameters.materialMask.SetVector($"_Sphere0{i+1}", sphereInfos[i]);   
        }
    }

    void AddSmokeMask(Transform transform)
    {
        if (intersectSmokeTransforms.Count<maxMasks)
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

     private void OnApplicationQuit()
    {
        ResetSpheres();
    }

}
