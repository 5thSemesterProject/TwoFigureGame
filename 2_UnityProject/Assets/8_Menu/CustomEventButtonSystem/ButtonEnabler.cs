using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ButtonEnabler : MonoBehaviour
{
    [HideInInspector]
    public CanvasGroup canvasGroup;
    public bool interactable = true;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }
}