using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

   [RequireComponent (typeof(Canvas),typeof(CanvasScaler))]
public class WSUI : MonoBehaviour
{
   Canvas canvas;
   static WSUI instance;
   static List<WSUI_Element> elements = new List<WSUI_Element>();

   void Awake()
   {
         if (instance==null)
            instance = this;
        else if (instance!=this)
            Destroy(instance);

         canvas = GetComponent<Canvas>();
   }

   public static void ShowPrompt (GameObject prefab,Transform transformToFollow,out GameObject spawnedElement)
   {
      spawnedElement = Instantiate(prefab);
      WSUI_Element element = spawnedElement.AddComponent<WSUI_Element>();
      element.SetCanvas(instance.canvas);
      element.SetTarget(transformToFollow);
      
      elements.Add (element);

      //Save scale
      Vector3 originalScale = spawnedElement.transform.localScale;

      //Set Scale
     // float scaleFactor = 3840/Screen.width;
      //spawnedElement.transform.localScale = originalScale/scaleFactor;
   } 

    public static void RemoveAllButtonPrompts()
    {
        for (int i = elements.Count - 1; i >= 0 ; i--)
        {
            DestroyPrompt(elements[i]);
        }
    }

   static void DestroyPrompt(WSUI_Element element)
   {
      Destroy(element);
      elements.Remove(element);
   }


}
