using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEditor;
using System.Runtime.InteropServices;

public class LightlayerDebugger : MonoBehaviour
{
    [SerializeField]Transform parentToSearchThrough;
    [SerializeField]LightLayerEnum lightlayerToSelect;

    [SerializeField] bool enabled;


    void OnDrawGizmos()
    {
        if (lightlayerToSelect == LightLayerEnum.Nothing) return;
        if (lightlayerToSelect == LightLayerEnum.Everything) return;

        if (parentToSearchThrough!=null &&enabled)
        {
            System.Array enumValues = System.Enum.GetValues(typeof(LightLayerEnum));
            Renderer[] rendererOnLightLayer = FilterByLightlayer(CalculateBitmask((int)lightlayerToSelect,enumValues),GetAllRenderers());
            
            for (int i = 0; i < rendererOnLightLayer.Length; i++)
            {
                Gizmos.color = Color.red;
                Renderer markedRenderer = rendererOnLightLayer[i];
                Mesh mesh;
                if (markedRenderer is MeshRenderer)
                    mesh = markedRenderer.GetComponent<MeshFilter>().sharedMesh;
                else
                {
                    var skinnedMeshRenderer = markedRenderer as SkinnedMeshRenderer;
                    mesh = skinnedMeshRenderer.sharedMesh;
                }
                    
                Gizmos.DrawMesh(mesh,markedRenderer.transform.position,markedRenderer.transform.rotation,markedRenderer.transform.localScale*1.1f);
            }
        }
            
    }

    Renderer[] FilterByLightlayer(int bitMask, Renderer[] renderers)
    {
        List<Renderer> filteredObjects = new List<Renderer>();
        System.Array enumValues = System.Enum.GetValues(typeof(LightLayerEnum));

        for (int i = 0; i < renderers.Length; i++)
        {   
           if (renderers[i].renderingLayerMask == (int)bitMask)
                filteredObjects.Add(renderers[i]);
        }

        return filteredObjects.ToArray();
    }

    Renderer[] GetAllRenderers()
    {
       return parentToSearchThrough.GetComponentsInChildren<Renderer>();
    }

    int CalculateBitmask(int currentBitmask, System.Array enumValues)
    {
            foreach (LightLayerEnum current in enumValues)
            {
                if (current == LightLayerEnum.Everything) continue;
 
                int layerBitVal = (int)current;
 
                bool set = current == lightlayerToSelect;
                currentBitmask = SetBitmask(currentBitmask, layerBitVal, set);
            }
 
            return currentBitmask;
    }
 
    int SetBitmask(int bitmask, int bitVal, bool set)
    {
            if (set)
                bitmask |= bitVal;
            else
                bitmask &= ~bitVal;
 
            return bitmask;
    }
}
