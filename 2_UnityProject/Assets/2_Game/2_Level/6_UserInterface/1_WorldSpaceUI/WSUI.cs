using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(Canvas),typeof(CanvasScaler))]
public class WSUI : MonoBehaviour
{
    private Canvas canvas;
    private static WSUI instance;
    private static List<WSUI_Element> elements = new List<WSUI_Element>();

    void Awake()
    {
         if (instance==null)
            instance = this;
        else if (instance!=this)
            Destroy(instance);

         canvas = GetComponent<Canvas>();
    }

    public static WSUI_Element ShowPrompt (GameObject prefab,Transform transformToFollow,out GameObject spawnedElement)
    {
        WSUI_Element element = SpawnIUElement(prefab);
        spawnedElement = element.gameObject;

        element.SetTarget(transformToFollow);

        return element;

        //Save scale
        //Vector3 originalScale = spawnedElement.transform.localScale;
        //
        //Set Scale
        //float scaleFactor = 3840/Screen.width;
        //spawnedElement.transform.localScale = originalScale/scaleFactor;
    }

    private static WSUI_Element SpawnIUElement(GameObject prefab)
    {
        GameObject spawnedElement = Instantiate(prefab);
        WSUI_Element element = spawnedElement.AddComponent<WSUI_Element>();
        element.SetCanvas(instance.canvas);

        elements.Add(element);

        return element;
    }

    public static void RemoveAllButtonPrompts()
    {
        for (int i = elements.Count - 1; i >= 0 ; i--)
        {
            RemovePrompt(elements[i]);
        }
    }

    public static void RemovePrompt(WSUI_Element element)
    {
      Destroy(element.gameObject);
      elements.Remove(element);
    }

    public static WSUI_Element AddOverlay(GameObject overlayPrefab)
    {
        WSUI_Element element = SpawnIUElement(overlayPrefab);
        element.SetScaleWithScreen();
        return element;
    }
}
