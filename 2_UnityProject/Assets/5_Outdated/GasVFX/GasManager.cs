using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[ExecuteInEditMode]
public class GasManager : MonoBehaviour
{   
    [SerializeField] LayerMask layerMask;
    Transform[] maskPos;
    int maxMasks = 3;

    VisualEffect visualEffect;

    void Awake()
    {
        
    }
    void Start()
    {
        ImportSmokeMasks();
        visualEffect = GetComponent<VisualEffect>();
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
                visualEffect.SetVector3($"MaskSphere_{i+1}_transform_position",transforms[i].position);
        }
    }

    void ImportSmokeMasks()
    {
        maskPos = new Transform[maxMasks];
        var allObjects = FindObjectsOfType<GameObject>() ;
        int counter = 0;
        for (int i = 0; i < 20; i++)
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
