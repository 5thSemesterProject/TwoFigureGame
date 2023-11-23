using UnityEngine;
using System.Collections;
using UnityEditor.SceneManagement;

class OccludingObject:MonoBehaviour
{
    public Coroutine coroutine;
    public  new Renderer renderer;
    float targetAlpha;


    private void Awake()
    {
        renderer = GetComponent<Renderer>();
    }

    public void LerpAlpha(float targetAlpha)
    {
        this.targetAlpha = targetAlpha;
        if (coroutine == null)
        {
            coroutine = StartCoroutine(_LerpAlpha());
        }
  
    }

    IEnumerator _LerpAlpha()
    {
        float currentAlpha = GetAlpha();
        float smoothTime = 0.33f;
        float velocity = 0;

        while (true)
        {
            currentAlpha = Mathf.SmoothDamp(currentAlpha, targetAlpha, ref velocity, smoothTime);

            if (Mathf.Abs(currentAlpha - targetAlpha) < 0.01f)
            {
                currentAlpha = targetAlpha;
                SetAlpha(currentAlpha);
                break;
            }

            SetAlpha(currentAlpha);

            yield return null;
        }

        coroutine = null;
        
    }

    public void SetAlpha(float inputAlpha)
    {
        if (renderer!=null)
        {
            Material material = renderer.materials[0];
            Vector4 color = material.GetVector("_BaseColor");
            color.w = inputAlpha;
            material.SetVector("_BaseColor", color);
        }

    }

    public float GetAlpha()
    {
        if (renderer != null)
        {
            Material material = renderer.materials[0];
            Vector4 color = material.GetVector("_BaseColor");
            return color.w;
        }
        else
            Debug.Log("Renderer is null on+ "+ gameObject.name);
        return 0;

    }

}