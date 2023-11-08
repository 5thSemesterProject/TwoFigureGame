using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.VFX;

public class VolumetricFogHandler : MonoBehaviour
{   
    [SerializeField] LayerMask layerMask;
    Vector4[] sphereInfo;
    int maxMasks = 3;

    LocalVolumetricFog localVolumetricFog;

    void Start()
    {
        ImportSmokeMasks();
        localVolumetricFog = GetComponent<LocalVolumetricFog>();
    }

    void Update()
    {
        SetSpheres(sphereInfo);
    }

    void SetSpheres(Vector4[] sphereInfoInput)
    {
        for (int i =0;i<maxMasks;i++)
        {
                float circleRadius  = localVolumetricFog.parameters.materialMask.GetVector($"_Sphere0{i+1}").w;
                localVolumetricFog.parameters.materialMask.SetVector($"_Sphere0{i+1}",sphereInfoInput[i]);       
        }
    }

    void ImportSmokeMasks()
    {
        sphereInfo = new Vector4[maxMasks];
        IEnumerable <IIntersectSmoke> findObjects = FindObjectsOfType<MonoBehaviour>().OfType<IIntersectSmoke>();
        List<IIntersectSmoke> allObjects = findObjects.ToList<IIntersectSmoke>();
        int counter = 0;
        for (int i = 0; i < allObjects.Count(); i++)
        {
                if (counter>=maxMasks)
                {
                    Debug.Log("There are more objects that mask out smoked then previsouly set");
                    break;
                }     
                sphereInfo[counter] = allObjects[i].GetSphereInformation();
                counter++;
        }
    }
}
