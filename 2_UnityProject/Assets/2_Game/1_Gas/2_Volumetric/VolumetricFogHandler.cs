using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.VFX;

public class VolumetricFogHandler : MonoBehaviour
{   
    [SerializeField] LayerMask layerMask;
    Transform[] maskPos;
    int maxMasks = 3;

    LocalVolumetricFog localVolumetricFog;

    void Awake()
    {
        
    }
    void Start()
    {
        ImportSmokeMasks();
        localVolumetricFog = GetComponent<LocalVolumetricFog>();
    }

    void Update()
    {
        SetTransforms(maskPos);
    }

    void SetTransforms(Transform[] transforms)
    {
        for (int i =0;i<maxMasks;i++)
        {
            if (transforms[i]!=null)
            {
                Vector3 circlePosition = transforms[i].position;
                float circlePosX = circlePosition.x;
                float circlePosY = circlePosition.y;
                float circlePosZ = circlePosition.z;
                float circleRadius  = localVolumetricFog.parameters.materialMask.GetVector($"_Sphere0{i+1}").w;
                localVolumetricFog.parameters.materialMask.SetVector($"_Sphere0{i+1}",new Vector4(circlePosX,circlePosY,circlePosZ,circleRadius));
                Debug.Log ("test");
            }
                
    }
    }

    void ImportSmokeMasks()
    {
        maskPos = new Transform[maxMasks];
        var allObjects = FindObjectsOfType<GameObject>() ;
        int counter = 0;
        for (int i = 0; i < allObjects.Length; i++)
        {
            if ((layerMask.value & (1 << allObjects[i].layer)) > 0)
            {
                
                if (counter>=maxMasks)
                {
                    Debug.Log("There are more objects that mask out smoked then previsouly set");
                    break;
                }     
                Debug.Log (allObjects[i].name);
                maskPos[counter] = allObjects[i].transform;
                counter++;
            }
        }
    }
}
