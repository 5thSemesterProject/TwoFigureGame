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

    public static WSUI_Element ShowPrompt (GameObject prefab,Transform transformToFollow,out WSUI_Element spawnedElement)
    {
        WSUI_Element element = SpawnIUElement(prefab);
        spawnedElement = element;

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

    public static void RemoveAndFadeOutPrompt(WSUI_Element element, float smoothTime = 0.33f)
    {
        element.StartCoroutine( _RemoveAndFadeOutPrompt(element,smoothTime));
    }

    static IEnumerator _RemoveAndFadeOutPrompt(WSUI_Element element, float smoothTime = 0.33f)
    {
        element.LerpAlpha(0,smoothTime);
        yield return new WaitUntil(()=>element.GetAlpha()<=0);
        elements.Remove(element);
        element.SetRemoved(true);
    }


    public static WSUI_Element AddOverlay(GameObject overlayPrefab)
    {
        WSUI_Element element = SpawnIUElement(overlayPrefab);
        element.SetScaleWithScreen();
        return element;
    }
}
