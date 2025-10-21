using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class StartPageVideo : MonoBehaviour
{
    [Header("Videos")]
    public VideoSource StartMoveVideo;
    public VideoSource ExitVideo;

    public MyButton StartButton;

    public VideoPlayer VideoPlayer;

    void Start()
    {
        VideoPlayer.source = StartMoveVideo;
        StartButton.OnDoubleClick.AddListener(ExitStartPage);

        StartButton.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        StartButton.OnDoubleClick.RemoveListener(ExitStartPage);
        VideoPlayer.loopPointReached -= OnStartMoveVideoFinished;
        VideoPlayer.loopPointReached -= OnExitVideoFinished;
    }

    private void ExitStartPage()
    {
        StartButton.gameObject.SetActive(false);
        
        VideoPlayer.loopPointReached += OnStartMoveVideoFinished;

        if (!VideoPlayer.isPlaying)
        {
            VideoPlayer.Play();
        }
    }

    private void OnStartMoveVideoFinished(VideoPlayer source)
    {
        VideoPlayer.loopPointReached -= OnStartMoveVideoFinished;

        VideoPlayer.source = ExitVideo;
        VideoPlayer.isLooping = false;
        VideoPlayer.loopPointReached += OnExitVideoFinished;

        VideoPlayer.Play();
    }

    private void OnExitVideoFinished(VideoPlayer source)
    {
        VideoPlayer.loopPointReached -= OnExitVideoFinished;
        SceneManager.LoadScene("FirstStagePage");
    }
}
