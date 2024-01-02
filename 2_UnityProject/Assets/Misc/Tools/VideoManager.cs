using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Video;

[RequireComponent(typeof(ButtonGroupFade))]
public class VideoManager:MonoBehaviour
{   
    [SerializeField] float skipPromptAppearance = 1;
    [SerializeField] Coroutine skipPromptProcess;
    [SerializeField] SkipPrompt skipPrompt;
    public UnityEvent videoFinished;
    public UnityEvent videoStarted;
    VideoPlayer videoPlayer;

    ButtonGroupFade buttonGroupFade;


    void  Awake()
    {
        videoPlayer = GetComponentInChildren<VideoPlayer>();
        buttonGroupFade = GetComponent<ButtonGroupFade>();

        GetComponent<CanvasGroup>().alpha = 0;
        
        if (skipPrompt!=null)
            skipPrompt.onSkip.AddListener(Skip);

    }

    public void StartVideo()
    {
        buttonGroupFade.FadeIn();
        if (videoPlayer!=null)
            videoPlayer.Play();
        StartCoroutine(WaitForVideoEnding());
        videoStarted?.Invoke();
    }

    void Skip()
    {
        StopAllCoroutines();
        videoFinished?.Invoke();
    }

    IEnumerator WaitForVideoEnding()
    {
        if (videoPlayer!=null)
        {
            float videoLength = (float)videoPlayer.length;
            yield return new WaitForSeconds(videoLength);
        }
        
        videoFinished?.Invoke();
    }

    public double GetWatchProgress()
    {
        return videoPlayer.time;
    }


}
