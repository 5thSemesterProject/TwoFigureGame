using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoManager : MonoBehaviour
{   
    [SerializeField] float skipPromptAppearance = 1;
    [SerializeField] Coroutine skipPromptProcess;
    [SerializeField] SkipPrompt skipPrompt;
    public UnityEvent videoFinished;
    public UnityEvent videoStarted;
    VideoPlayer videoPlayer;


    void  Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    public void StartVideo()
    {
        videoPlayer.Play();
        StartCoroutine(WaitForVideoEnding());
        videoStarted?.Invoke();
    }

    void Skip(InputAction.CallbackContext callbackContext)
    {
        StopAllCoroutines();
        videoFinished?.Invoke();
    }

    IEnumerator WaitForVideoEnding()
    {
        float videoLength = (float)videoPlayer.length;
        yield return new WaitForSeconds(videoLength);
        videoFinished?.Invoke();
    }

    public double GetWatchProgress()
    {
        return videoPlayer.time;
    }


}
