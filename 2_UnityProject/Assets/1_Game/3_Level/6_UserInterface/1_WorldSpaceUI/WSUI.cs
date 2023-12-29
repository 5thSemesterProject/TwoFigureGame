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

    public static void FadeInElement(GameObject prefab,Transform transformToFollow,out WSUI_Element spawnedElement)
    {
        ShowElement(prefab,transformToFollow,out spawnedElement);
        spawnedElement.SetAlpha(0);
        spawnedElement.LerpAlpha(1);
    }

    public static void ShowElement (GameObject prefab,Transform transformToFollow,out WSUI_Element spawnedElement)
    {
        spawnedElement = SpawnWSUIElement(prefab);
        spawnedElement.SetTarget(transformToFollow);
    }

    private static WSUI_Element SpawnWSUIElement(GameObject prefab)
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

    static IEnumerator _RemoveAndFadeOutPrompt(WSUI_Element element, float smoothTime)
    {
        element.SetTargetAlpha(0);
        yield return element._LerpAlpha(smoothTime);
        elements.Remove(element);
        element.SetRemoved(true);
        Destroy(element.gameObject);
    }


    public static WSUI_Element AddOverlay(GameObject overlayPrefab)
    {
        WSUI_Element element = SpawnWSUIElement(overlayPrefab);
        element.SetCenterScreen();
        return element;
    }
}
