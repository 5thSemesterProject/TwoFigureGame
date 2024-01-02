using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadScreenManager : MonoBehaviour
{
    //References
    [SerializeField] private RectTransform loadingIcon;
    [SerializeField] private RectTransform pressToLoadText;

    //Button event is a void event with no parameters
    public event ButtonEvent onLoad;

    public void LoadLevel()
    {
        onLoad?.Invoke();
    }

    public void SignalLoad()
    {
        loadingIcon.gameObject.SetActive(false);

        pressToLoadText.gameObject.SetActive(true);
    }

    public void SignalLoading()
    {
        loadingIcon.gameObject.SetActive(true);

        pressToLoadText.gameObject.SetActive(false);
    }
}
