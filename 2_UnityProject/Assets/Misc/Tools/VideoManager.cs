using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoManager : MonoBehaviour
{   
    public UnityEvent videoPlayed;
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

    IEnumerator WaitForVideoEnding()
    {
        float videoLength = (float)videoPlayer.length;
        yield return new WaitForSeconds(videoLength);
        videoPlayed?.Invoke();
    }

    public double GetWatchProgress()
    {
        return videoPlayer.time;
    }
}
