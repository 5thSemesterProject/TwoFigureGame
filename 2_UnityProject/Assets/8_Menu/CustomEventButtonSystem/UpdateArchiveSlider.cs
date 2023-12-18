using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class UpdateArchiveSlider : MonoBehaviour
{
    private Slider archiveProgress;
    [SerializeField] private int maxObjects;
    [SerializeField] private int ownedObjects;

    private void OnEnable()
    {
        archiveProgress = GetComponent<Slider>();
        UpdateProgress();
    }

    public void UpdateProgress()
    {
        UpdateSlider((float)ownedObjects / (float)maxObjects);
    }

    public void UpdateSlider(float value)
    {
        value = Mathf.Clamp01(value);
        archiveProgress.value = value;
    }
}
