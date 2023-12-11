using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WSUI_Element : MonoBehaviour
{

    Coroutine coroutine;
    Transform transformToFollow;
    Canvas canvas;
    RectTransform rectTransform;
    Vector2 offset = new Vector2(0,0);

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

   public void SetTarget(Transform transformToFollow)
   {
        this.transformToFollow = transformToFollow;
        _FollowTransform(offset);
        coroutine = StartCoroutine(FollowTransform(offset));
   }

   public void SetOffset(Vector2 offset)
   {
        this.offset = offset;
   }

   public void SetCanvas(Canvas canvas)
   {
        this.canvas = canvas;
        rectTransform.SetParent(canvas.GetComponent<RectTransform>());
   }

    public void SetScaleWithScreen()
    {
        rectTransform.anchoredPosition = Vector3.zero;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
    }

    IEnumerator FollowTransform(Vector2 offset)
   {
        while (true)
        {
            _FollowTransform(offset);
            yield return null;
        }
   }
    
    void _FollowTransform(Vector2 offset)
    {   
        //Calculate Scaling
        CanvasScaler canvasScaler = canvas.GetComponent<CanvasScaler>();
        float xScreenToScaler = canvasScaler.referenceResolution.x /Screen.width;
        float yScreenToScaler = canvasScaler.referenceResolution.y / Screen.height;

        //Transform from wold to Screen
        Vector3 worldToScreenPoint = Camera.main.WorldToScreenPoint(transformToFollow.position);
        Vector2 screenMidPoint = new Vector2(Screen.width * xScreenToScaler, Screen.height * yScreenToScaler) /2;
        Vector2 newPos = new Vector2((worldToScreenPoint.x + offset.x) * xScreenToScaler, (worldToScreenPoint.y + offset.y) * yScreenToScaler) - screenMidPoint;
        
        //Set position
        rectTransform.anchoredPosition = newPos;
   }
   


}
