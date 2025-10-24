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
    public VideoClip StartMoveVideo;
    public VideoClip ExitVideo;

    public MyButton StartButton;

    public VideoPlayer VideoPlayer;

    //private bool isTransitioning = false;

    void Start()
    {
        VideoPlayer.clip = StartMoveVideo;
        VideoPlayer.isLooping = true;
        VideoPlayer.Play();

        StartButton.OnDoubleClick.AddListener(ExitStartPage);
    }

    private void ExitStartPage()
    {
        StartButton.gameObject.SetActive(false);

        StartCoroutine(PlayExitVideoAndLoadScene());
    }

    private IEnumerator PlayExitVideoAndLoadScene()
    {
        // 播放退出视频
        VideoPlayer.clip = ExitVideo;
        VideoPlayer.isLooping = false;
        VideoPlayer.Play();

        // 等待视频长度的时间
        yield return new WaitForSeconds((float)ExitVideo.length);

        // 加载场景
        SceneManager.LoadScene("FirstStagePage");
    }









    //void Start()
    //{
    //    VideoPlayer.clip = StartMoveVideo;
    //    VideoPlayer.isLooping = true;
    //    VideoPlayer.Play();

    //    StartButton.OnDoubleClick.AddListener(ExitStartPage);
    //    StartButton.gameObject.SetActive(true);
    //}

    //private void OnDestroy()
    //{
    //    StartButton.OnDoubleClick.RemoveListener(ExitStartPage);
    //    VideoPlayer.loopPointReached -= OnStartMoveVideoFinished;
    //    //VideoPlayer.loopPointReached -= OnExitVideoFinished;
    //}

    //private void ExitStartPage()
    //{
    //    if (isTransitioning) return;

    //    isTransitioning = true;
    //    StartButton.gameObject.SetActive(false);

    //    VideoPlayer.loopPointReached -= OnStartMoveVideoFinished;
    //    VideoPlayer.loopPointReached += OnStartMoveVideoFinished;

    //    VideoPlayer.clip = ExitVideo;
    //    VideoPlayer.isLooping = false;
    //    VideoPlayer.Play();

    //    Debug.Log("开始播放退出视频");
    //}

    //private void OnStartMoveVideoFinished(VideoPlayer source)
    //{
    //    Debug.Log("开始移动视频播放完成");
    //    VideoPlayer.loopPointReached -= OnStartMoveVideoFinished;

    //    SceneManager.LoadScene("FirstStagePage");
    //}

    ////private void OnExitVideoFinished(VideoPlayer source)
    ////{
    ////    VideoPlayer.loopPointReached -= OnExitVideoFinished;
    ////    SceneManager.LoadScene("FirstStagePage");
    ////}
}
